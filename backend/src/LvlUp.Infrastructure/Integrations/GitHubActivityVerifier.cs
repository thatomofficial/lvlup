using System.Text.Json;
using LvlUp.Application.Abstractions.Integrations;

namespace LvlUp.Infrastructure.Integrations;

/// <summary>
/// Checks the public GitHub events feed for push activity. Uses the
/// unauthenticated API (60 requests/hour), which only sees public events -
/// pushes to private repositories will not verify.
/// </summary>
internal sealed class GitHubActivityVerifier(HttpClient httpClient) : IGitHubActivityVerifier
{
    public async Task<bool> HasPushedOnDateAsync(string username, DateOnly utcDate, CancellationToken cancellationToken)
    {
        var requestUri = new Uri(
            $"users/{Uri.EscapeDataString(username)}/events/public?per_page=100",
            UriKind.Relative);

        using HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Fail closed: an unreachable feed cannot verify a push.
            return false;
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        foreach (JsonElement gitHubEvent in document.RootElement.EnumerateArray())
        {
            if (!gitHubEvent.TryGetProperty("type", out JsonElement type) ||
                type.GetString() != "PushEvent")
            {
                continue;
            }

            if (gitHubEvent.TryGetProperty("created_at", out JsonElement createdAt) &&
                createdAt.TryGetDateTimeOffset(out DateTimeOffset timestamp) &&
                DateOnly.FromDateTime(timestamp.UtcDateTime) == utcDate)
            {
                return true;
            }
        }

        return false;
    }
}
