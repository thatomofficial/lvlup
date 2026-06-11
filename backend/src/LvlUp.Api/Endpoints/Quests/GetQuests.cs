using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Quests;
using LvlUp.Application.Quests.GetQuests;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Quests;

internal sealed class GetQuests : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("quests", async (
            IUserContext userContext,
            IQueryHandler<GetQuestsQuery, IReadOnlyList<QuestResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetQuestsQuery(userContext.UserId);

            Result<IReadOnlyList<QuestResponse>> result = await handler.HandleAsync(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.QuestsRead)
        .WithTags(Tags.Quests);
    }
}
