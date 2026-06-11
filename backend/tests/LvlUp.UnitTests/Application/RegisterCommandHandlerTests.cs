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
        RegisterCommand command = CreateCommand();

        await _handler.HandleAsync(command, CancellationToken.None);

        _context.Hunters.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnToken_WhenRegistrationSucceeds()
    {
        RegisterCommand command = CreateCommand();

        Result<AuthResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Value.Token.ShouldBe("jwt-token");
    }

    [Fact]
    public async Task HandleAsync_Should_NormalizeEmailToLowercase()
    {
        RegisterCommand command = CreateCommand(email: "New@LvlUp.App");

        await _handler.HandleAsync(command, CancellationToken.None);

        _context.Hunters.Single().Email.ShouldBe("new@lvlup.app");
    }

    [Fact]
    public async Task HandleAsync_Should_NormalizeUsernameToLowercase()
    {
        RegisterCommand command = CreateCommand(username: "Shadow_Monarch");

        await _handler.HandleAsync(command, CancellationToken.None);

        _context.Hunters.Single().Username.ShouldBe("shadow_monarch");
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_WhenEmailAlreadyExists()
    {
        await _handler.HandleAsync(CreateCommand(), CancellationToken.None);

        Result<AuthResponse> result = await _handler.HandleAsync(
            CreateCommand(username: "other_hunter"),
            CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.EmailNotUnique);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_WhenUsernameAlreadyExists()
    {
        await _handler.HandleAsync(CreateCommand(), CancellationToken.None);

        Result<AuthResponse> result = await _handler.HandleAsync(
            CreateCommand(email: "other@lvlup.app"),
            CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.UsernameNotUnique);
    }

    public void Dispose() => _context.Dispose();

    private static RegisterCommand CreateCommand(
        string email = "new@lvlup.app",
        string username = "shadow_monarch") =>
        new(email, "password123", "Jin-Woo", "Sung", username);
}
