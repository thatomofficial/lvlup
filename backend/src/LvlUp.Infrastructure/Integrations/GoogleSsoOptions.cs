namespace LvlUp.Infrastructure.Integrations;

public sealed class GoogleSsoOptions
{
    public const string SectionName = "Sso:Google";

    /// <summary>OAuth client id the ID token must be issued for. SSO is disabled while empty.</summary>
    public string ClientId { get; init; } = string.Empty;

    public string TokenInfoBaseUrl { get; init; } = "https://oauth2.googleapis.com/";
}
