using Microsoft.Extensions.Options;

namespace LvlUp.Api.Endpoints.App;

internal sealed class GetAppConfig : IEndpoint
{
    public sealed record Response(string MinimumVersion, string LatestVersion);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("app/config", (IOptions<AppVersionOptions> options) =>
            Results.Ok(new Response(options.Value.MinimumVersion, options.Value.LatestVersion)))
        .AllowAnonymous()
        .WithTags(Tags.App);
    }
}
