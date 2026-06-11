using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Quests.DeleteQuest;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Quests;

internal sealed class DeleteQuest : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("quests/{questId:guid}", async (
            Guid questId,
            IUserContext userContext,
            ICommandHandler<DeleteQuestCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteQuestCommand(userContext.UserId, questId);

            Result result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .HasPermission(Permissions.QuestsManage)
        .WithTags(Tags.Quests);
    }
}
