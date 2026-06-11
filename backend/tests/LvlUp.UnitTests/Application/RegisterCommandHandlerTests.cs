using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Auth;
using LvlUp.Application.Auth.Register;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using NSubstitute;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class RegisterCommandHandlerTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 11, 8, 0, 0, TimeSpan.Zero);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenProvider _tokenProvider = Substitute.For<ITokenProvider>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");
        _tokenProvider.Create(Arg.Any<Hunter>()).Returns("jwt-token");

        _handler = new RegisterCommandHandler(
            _context,
            _passwordHasher,
            _tokenProvider,
            new FixedTimeProvider(UtcNow));
    }

    [Fact]
    public async Task HandleAsync_Should_CreateHunter_WhenEmailIsUnique()
    {
        var command = new RegisterCommand("new@lvlup.app", "password123", "Jin-Woo");

        await _handler.HandleAsync(command, CancellationToken.None);

        _context.Hunters.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnToken_WhenRegistrationSucceeds()
    {
        var command = new RegisterCommand("new@lvlup.app", "password123", "Jin-Woo");

        Result<AuthResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Value.Token.ShouldBe("jwt-token");
    }

    [Fact]
    public async Task HandleAsync_Should_NormalizeEmailToLowercase()
    {
        var command = new RegisterCommand("New@LvlUp.App", "password123", "Jin-Woo");

        await _handler.HandleAsync(command, CancellationToken.None);

        _context.Hunters.Single().Email.ShouldBe("new@lvlup.app");
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_WhenEmailAlreadyExists()
    {
        var command = new RegisterCommand("taken@lvlup.app", "password123", "Jin-Woo");
        await _handler.HandleAsync(command, CancellationToken.None);

        Result<AuthResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.EmailNotUnique);
    }

    public void Dispose() => _context.Dispose();
}
