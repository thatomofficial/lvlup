using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Hunters.UpdateDisplayNamePreference;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Hunters;

internal sealed class UpdateDisplayPreference : IEndpoint
{
    public sealed record Request(DisplayNamePreference Preference);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("hunters/me/display-preference", async (
            Request request,
            IUserContext userContext,
            ICommandHandler<UpdateDisplayNamePreferenceCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateDisplayNamePreferenceCommand(userContext.UserId, request.Preference);

            Result result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .HasPermission(Permissions.HuntersRead)
        .WithTags(Tags.Hunters);
    }
}
