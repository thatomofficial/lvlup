using System.Text.Json;
using LvlUp.Application.Abstractions.Integrations;
using Microsoft.Extensions.Options;

namespace LvlUp.Infrastructure.Integrations;

/// <summary>
/// Verifies Google ID tokens via the tokeninfo endpoint (Google validates the
/// signature and expiry; we validate the audience and that the email is verified).
/// </summary>
internal sealed class GoogleIdTokenVerifier(HttpClient httpClient, IOptions<GoogleSsoOptions> options)
    : IGoogleIdTokenVerifier
{
    public async Task<GoogleUserInfo?> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        string clientId = options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return null;
        }

        var requestUri = new Uri($"tokeninfo?id_token={Uri.EscapeDataString(idToken)}", UriKind.Relative);

        using HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        JsonElement root = document.RootElement;

        if (GetString(root, "aud") != clientId ||
            GetString(root, "email_verified") != "true" ||
            GetString(root, "email") is not { Length: > 0 } email)
        {
            return null;
        }

        return new GoogleUserInfo(email, GetString(root, "given_name"), GetString(root, "family_name"));
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
}
