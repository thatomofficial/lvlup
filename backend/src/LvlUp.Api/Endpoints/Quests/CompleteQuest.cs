using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Quests.CompleteQuest;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Quests;

internal sealed class CompleteQuest : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("quests/{questId:guid}/complete", async (
            Guid questId,
            IUserContext userContext,
            ICommandHandler<CompleteQuestCommand, CompleteQuestResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteQuestCommand(userContext.UserId, questId);

            Result<CompleteQuestResponse> result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.QuestsManage)
        .WithTags(Tags.Quests);
    }
}
