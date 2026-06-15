namespace LvlUp.Application.Abstractions.Integrations;

public sealed record GoogleUserInfo(string Email, string? GivenName, string? FamilyName);

/// <summary>
/// Verifies a Google Sign-In ID token and returns the verified profile,
/// or null when the token is invalid, expired, or SSO is not configured.
/// </summary>
public interface IGoogleIdTokenVerifier
{
    Task<GoogleUserInfo?> VerifyAsync(string idToken, CancellationToken cancellationToken);
}
