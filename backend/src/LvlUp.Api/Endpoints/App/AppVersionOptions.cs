namespace LvlUp.Api.Endpoints.App;

public sealed class AppVersionOptions
{
    public const string SectionName = "App";

    /// <summary>Clients below this version are blocked at boot until they update.</summary>
    public string MinimumVersion { get; init; } = "1.0.0";

    public string LatestVersion { get; init; } = "1.0.0";
}
