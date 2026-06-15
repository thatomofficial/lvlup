using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Notifications;
using LvlUp.Application.Auth.RequestPasswordReset;
using LvlUp.Application.Auth.ResetPassword;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using NSubstitute;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class PasswordResetTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly IOtpGenerator _otpGenerator = Substitute.For<IOtpGenerator>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IPasswordResetNotifier _notifier = Substitute.For<IPasswordResetNotifier>();
    private readonly FixedTimeProvider _timeProvider = new(UtcNow);
    private readonly RequestPasswordResetCommandHandler _requestHandler;
    private readonly ResetPasswordCommandHandler _resetHandler;

    public PasswordResetTests()
    {
        // Deterministic, reversible fakes: hash prefixes the value, verify checks the prefix.
        _otpGenerator.Generate().Returns("123456");
        _passwordHasher.Hash(Arg.Any<string>()).Returns(call => $"hash:{call.Arg<string>()}");
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"hash:{call.ArgAt<string>(0)}" == call.ArgAt<string>(1));

        _requestHandler = new RequestPasswordResetCommandHandler(
            _context, _otpGenerator, _passwordHasher, _notifier, _timeProvider);
        _resetHandler = new ResetPasswordCommandHandler(_context, _passwordHasher, _timeProvider);
    }

    [Fact]
    public async Task Request_Should_Succeed_AndNotNotify_ForUnknownEmail()
    {
        Result result = await _requestHandler.HandleAsync(
            new RequestPasswordResetCommand("nobody@lvlup.app"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _notifier.DidNotReceiveWithAnyArgs().SendCodeAsync(default!, default!, default, default);
    }

    [Fact]
    public async Task Request_Should_StoreHashedCode_AndNotify_ForKnownEmail()
    {
        await SeedHunterAsync();

        Result result = await _requestHandler.HandleAsync(
            new RequestPasswordResetCommand("hunter@lvlup.app"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        Hunter hunter = _context.Hunters.Single();
        hunter.ShouldSatisfyAllConditions(
            h => h.PasswordResetCodeHash.ShouldBe("hash:123456"),
            h => h.IsPasswordResetActive(UtcNow.UtcDateTime).ShouldBeTrue());
        await _notifier.Received(1).SendCodeAsync(
            "hunter@lvlup.app", "123456", Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reset_Should_ChangePassword_WithValidCode()
    {
        await SeedHunterAsync();
        await _requestHandler.HandleAsync(
            new RequestPasswordResetCommand("hunter@lvlup.app"), CancellationToken.None);

        Result result = await _resetHandler.HandleAsync(
            new ResetPasswordCommand("hunter@lvlup.app", "123456", "new-password"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        Hunter hunter = _context.Hunters.Single();
        hunter.ShouldSatisfyAllConditions(
            h => h.PasswordHash.ShouldBe("hash:new-password"),
            h => h.PasswordResetCodeHash.ShouldBeNull());
    }

    [Fact]
    public async Task Reset_Should_Fail_WithWrongCode_AndCountAttempt()
    {
        await SeedHunterAsync();
        await _requestHandler.HandleAsync(
            new RequestPasswordResetCommand("hunter@lvlup.app"), CancellationToken.None);

        Result result = await _resetHandler.HandleAsync(
            new ResetPasswordCommand("hunter@lvlup.app", "000000", "new-password"),
            CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.InvalidPasswordResetCode);
        _context.Hunters.Single().PasswordResetFailedAttempts.ShouldBe(1);
    }

    [Fact]
    public async Task Reset_Should_Fail_WhenNoResetRequested()
    {
        await SeedHunterAsync();

        Result result = await _resetHandler.HandleAsync(
            new ResetPasswordCommand("hunter@lvlup.app", "123456", "new-password"),
            CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.InvalidPasswordResetCode);
    }

    [Fact]
    public async Task Reset_Should_Fail_WhenCodeExpired()
    {
        await SeedHunterAsync();
        await _requestHandler.HandleAsync(
            new RequestPasswordResetCommand("hunter@lvlup.app"), CancellationToken.None);

        _timeProvider.Advance(TimeSpan.FromMinutes(16));

        Result result = await _resetHandler.HandleAsync(
            new ResetPasswordCommand("hunter@lvlup.app", "123456", "new-password"),
            CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.InvalidPasswordResetCode);
    }

    [Fact]
    public async Task Reset_Should_Fail_AfterTooManyAttempts()
    {
        await SeedHunterAsync();
        await _requestHandler.HandleAsync(
            new RequestPasswordResetCommand("hunter@lvlup.app"), CancellationToken.None);

        for (int i = 0; i < Hunter.MaxPasswordResetAttempts; i++)
        {
            await _resetHandler.HandleAsync(
                new ResetPasswordCommand("hunter@lvlup.app", "000000", "new-password"),
                CancellationToken.None);
        }

        // Even the correct code is now rejected; the reset window is burned.
        Result result = await _resetHandler.HandleAsync(
            new ResetPasswordCommand("hunter@lvlup.app", "123456", "new-password"),
            CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.InvalidPasswordResetCode);
        _context.Hunters.Single().PasswordResetCodeHash.ShouldBeNull();
    }

    public void Dispose() => _context.Dispose();

    private async Task SeedHunterAsync()
    {
        var hunter = Hunter.Create(
            "hunter@lvlup.app", "hash:old-password", "Jin-Woo", "Sung", "shadow_monarch", UtcNow.UtcDateTime);
        _context.Hunters.Add(hunter);
        await _context.SaveChangesAsync();
    }
}
