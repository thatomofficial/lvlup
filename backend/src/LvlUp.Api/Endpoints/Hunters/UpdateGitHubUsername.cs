using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Hunters.UpdateGitHubUsername;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Hunters;

internal sealed class UpdateGitHubUsername : IEndpoint
{
    public sealed record Request(string? Username);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("hunters/me/github", async (
            Request request,
            IUserContext userContext,
            ICommandHandler<UpdateGitHubUsernameCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateGitHubUsernameCommand(userContext.UserId, request.Username);

            Result result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .HasPermission(Permissions.HuntersRead)
        .WithTags(Tags.Hunters);
    }
}
