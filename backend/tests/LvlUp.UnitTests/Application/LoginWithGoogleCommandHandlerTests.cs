using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Integrations;
using LvlUp.Application.Auth;
using LvlUp.Application.Auth.LoginWithGoogle;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using NSubstitute;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class LoginWithGoogleCommandHandlerTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 12, 8, 0, 0, TimeSpan.Zero);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly IGoogleIdTokenVerifier _verifier = Substitute.For<IGoogleIdTokenVerifier>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenProvider _tokenProvider = Substitute.For<ITokenProvider>();
    private readonly LoginWithGoogleCommandHandler _handler;

    public LoginWithGoogleCommandHandlerTests()
    {
        _passwordHasher.Hash(Arg.Any<string>()).Returns("random-hash");
        _tokenProvider.Create(Arg.Any<Hunter>()).Returns("jwt-token");

        _handler = new LoginWithGoogleCommandHandler(
            _context,
            _verifier,
            _passwordHasher,
            _tokenProvider,
            new FixedTimeProvider(UtcNow));
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnUnauthorized_WhenTokenIsInvalid()
    {
        _verifier.VerifyAsync("bad-token", Arg.Any<CancellationToken>()).Returns((GoogleUserInfo?)null);

        Result<AuthResponse> result = await _handler.HandleAsync(
            new LoginWithGoogleCommand("bad-token"),
            CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.InvalidSsoToken);
    }

    [Fact]
    public async Task HandleAsync_Should_CreateHunter_OnFirstSignIn()
    {
        SetupVerifiedUser("jinwoo@gmail.com", "Jin-Woo", "Sung");

        await _handler.HandleAsync(new LoginWithGoogleCommand("token"), CancellationToken.None);

        _context.Hunters.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task HandleAsync_Should_DeriveUsernameFromEmail()
    {
        SetupVerifiedUser("jinwoo@gmail.com", "Jin-Woo", "Sung");

        await _handler.HandleAsync(new LoginWithGoogleCommand("token"), CancellationToken.None);

        _context.Hunters.Single().Username.ShouldBe("jinwoo");
    }

    [Fact]
    public async Task HandleAsync_Should_SuffixUsername_WhenTaken()
    {
        var existing = Hunter.Create("other@lvlup.app", "hash", "Other", "Hunter", "jinwoo", UtcNow.UtcDateTime);
        _context.Hunters.Add(existing);
        await _context.SaveChangesAsync();
        SetupVerifiedUser("jinwoo@gmail.com", "Jin-Woo", "Sung");

        await _handler.HandleAsync(new LoginWithGoogleCommand("token"), CancellationToken.None);

        Hunter created = _context.Hunters.Single(h => h.Email == "jinwoo@gmail.com");
        created.Username.ShouldSatisfyAllConditions(
            username => username.ShouldStartWith("jinwoo_"),
            username => username.Length.ShouldBeLessThanOrEqualTo(30));
    }

    [Fact]
    public async Task HandleAsync_Should_ReuseExistingHunter_ByEmail()
    {
        var existing = Hunter.Create("jinwoo@gmail.com", "hash", "Jin-Woo", "Sung", "shadow_monarch", UtcNow.UtcDateTime);
        _context.Hunters.Add(existing);
        await _context.SaveChangesAsync();
        SetupVerifiedUser("jinwoo@gmail.com", "Jin-Woo", "Sung");

        Result<AuthResponse> result = await _handler.HandleAsync(
            new LoginWithGoogleCommand("token"),
            CancellationToken.None);

        result.Value.HunterId.ShouldBe(existing.Id);
    }

    [Fact]
    public async Task HandleAsync_Should_FallBackToDefaultName_WhenProfileHasNone()
    {
        SetupVerifiedUser("mystery@gmail.com", null, null);

        await _handler.HandleAsync(new LoginWithGoogleCommand("token"), CancellationToken.None);

        _context.Hunters.Single().Name.ShouldBe("Hunter");
    }

    public void Dispose() => _context.Dispose();

    private void SetupVerifiedUser(string email, string? givenName, string? familyName) =>
        _verifier.VerifyAsync("token", Arg.Any<CancellationToken>())
            .Returns(new GoogleUserInfo(email, givenName, familyName));
}
