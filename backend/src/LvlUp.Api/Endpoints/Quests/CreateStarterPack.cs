using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Quests.CreateStarterPack;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Quests;

internal sealed class CreateStarterPack : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("quests/starter-pack", async (
            IUserContext userContext,
            ICommandHandler<CreateStarterPackCommand, CreateStarterPackResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateStarterPackCommand(userContext.UserId);

            Result<CreateStarterPackResponse> result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.QuestsManage)
        .WithTags(Tags.Quests);
    }
}
