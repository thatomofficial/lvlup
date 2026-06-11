using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Quests.CreateQuest;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Quests;

internal sealed class CreateQuest : IEndpoint
{
    public sealed record Request(
        string Title,
        string? Description,
        StatCategory Category,
        QuestDifficulty Difficulty,
        QuestType Type);

    public sealed record Response(Guid Id);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("quests", async (
            Request request,
            IUserContext userContext,
            ICommandHandler<CreateQuestCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateQuestCommand(
                userContext.UserId,
                request.Title,
                request.Description,
                request.Category,
                request.Difficulty,
                request.Type);

            Result<Guid> result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(id => Results.Ok(new Response(id)), CustomResults.Problem);
        })
        .HasPermission(Permissions.QuestsManage)
        .WithTags(Tags.Quests);
    }
}
