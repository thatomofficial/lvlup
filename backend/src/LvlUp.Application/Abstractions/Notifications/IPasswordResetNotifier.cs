namespace LvlUp.Application.Abstractions.Notifications;

/// <summary>
/// Delivers a password-reset code to the hunter (email/SMS/...). The default
/// adapter logs the code for local development; swap it for a real email
/// sender in production behind this same interface.
/// </summary>
public interface IPasswordResetNotifier
{
    Task SendCodeAsync(string email, string code, DateTime expiresAtUtc, CancellationToken cancellationToken);
}
