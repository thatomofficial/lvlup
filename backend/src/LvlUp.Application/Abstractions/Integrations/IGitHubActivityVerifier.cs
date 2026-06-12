namespace LvlUp.Application.Abstractions.Integrations;

public interface IGitHubActivityVerifier
{
    /// <summary>Returns true when the user has at least one public push event on the given UTC date.</summary>
    Task<bool> HasPushedOnDateAsync(string username, DateOnly utcDate, CancellationToken cancellationToken);
}
