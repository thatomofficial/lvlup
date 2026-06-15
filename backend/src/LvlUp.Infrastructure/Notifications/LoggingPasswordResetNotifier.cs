using LvlUp.Application.Abstractions.Notifications;
using Microsoft.Extensions.Logging;

namespace LvlUp.Infrastructure.Notifications;

/// <summary>
/// Development notifier: writes the reset code to the log instead of sending an
/// email. Replace with a real email/SMS adapter (SendGrid, SMTP, ...) behind
/// <see cref="IPasswordResetNotifier"/> for production.
/// </summary>
internal sealed class LoggingPasswordResetNotifier(ILogger<LoggingPasswordResetNotifier> logger)
    : IPasswordResetNotifier
{
    public Task SendCodeAsync(string email, string code, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "PASSWORD RESET (dev): code {Code} for {Email}, expires {ExpiresAtUtc:u}. " +
            "Configure a real notifier before production.",
            code,
            email,
            expiresAtUtc);

        return Task.CompletedTask;
    }
}
