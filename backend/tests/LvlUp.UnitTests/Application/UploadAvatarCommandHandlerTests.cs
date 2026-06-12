using LvlUp.Application.Abstractions.Storage;
using LvlUp.Application.Hunters.UploadAvatar;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using NSubstitute;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class UploadAvatarCommandHandlerTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 12, 8, 0, 0, TimeSpan.Zero);
    private static readonly byte[] Png = [1, 2, 3];

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly IFileStorage _fileStorage = Substitute.For<IFileStorage>();
    private readonly UploadAvatarCommandHandler _handler;

    public UploadAvatarCommandHandlerTests()
    {
        _fileStorage
            .SaveAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => $"{callInfo.ArgAt<string>(0)}/{callInfo.ArgAt<string>(1)}");

        _handler = new UploadAvatarCommandHandler(_context, _fileStorage);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenHunterDoesNotExist()
    {
        var command = new UploadAvatarCommand(Guid.NewGuid(), Png, "image/png");

        Result<UploadAvatarResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Error.Code.ShouldBe("Hunters.NotFound");
    }

    [Fact]
    public async Task HandleAsync_Should_RejectUnsupportedContentType()
    {
        Hunter hunter = await SeedHunterAsync();
        var command = new UploadAvatarCommand(hunter.Id, Png, "image/gif");

        Result<UploadAvatarResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.UnsupportedAvatarType);
    }

    [Fact]
    public async Task HandleAsync_Should_StoreFileAndPersistPath()
    {
        Hunter hunter = await SeedHunterAsync();
        var command = new UploadAvatarCommand(hunter.Id, Png, "image/png");

        await _handler.HandleAsync(command, CancellationToken.None);

        hunter.AvatarPath.ShouldStartWith("avatars/");
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnPublicUrl()
    {
        Hunter hunter = await SeedHunterAsync();
        var command = new UploadAvatarCommand(hunter.Id, Png, "image/png");

        Result<UploadAvatarResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Value.AvatarUrl.ShouldStartWith("/files/avatars/");
    }

    [Fact]
    public async Task HandleAsync_Should_DeletePreviousAvatar_WhenReplacing()
    {
        Hunter hunter = await SeedHunterAsync();
        await _handler.HandleAsync(new UploadAvatarCommand(hunter.Id, Png, "image/png"), CancellationToken.None);
        string firstPath = hunter.AvatarPath!;

        await _handler.HandleAsync(new UploadAvatarCommand(hunter.Id, Png, "image/jpeg"), CancellationToken.None);

        await _fileStorage.Received(1).DeleteAsync(firstPath, Arg.Any<CancellationToken>());
    }

    public void Dispose() => _context.Dispose();

    private async Task<Hunter> SeedHunterAsync()
    {
        var hunter = Hunter.Create("hunter@lvlup.app", "hash", "Jin-Woo", "Sung", "shadow_monarch", UtcNow.UtcDateTime);
        _context.Hunters.Add(hunter);
        await _context.SaveChangesAsync();
        return hunter;
    }
}
