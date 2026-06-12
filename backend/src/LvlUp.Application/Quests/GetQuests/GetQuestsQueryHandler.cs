using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Quests.GetQuests;

internal sealed class GetQuestsQueryHandler(IApplicationDbContext context, TimeProvider timeProvider)
    : IQueryHandler<GetQuestsQuery, IReadOnlyList<QuestResponse>>
{
    public async Task<Result<IReadOnlyList<QuestResponse>>> HandleAsync(
        GetQuestsQuery query,
        CancellationToken cancellationToken)
    {
        List<Quest> quests = await context.Quests
            .AsNoTracking()
            .Where(quest => quest.HunterId == query.HunterId)
            .OrderBy(quest => quest.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        IReadOnlyList<QuestResponse> responses =
        [
            .. quests.Select(quest => new QuestResponse
            {
                Id = quest.Id,
                Title = quest.Title,
                Description = quest.Description,
                Category = quest.Category,
                Difficulty = quest.Difficulty,
                Type = quest.Type,
                Verification = quest.Verification,
                XpReward = quest.XpReward,
                StatReward = quest.StatReward,
                IsCompleted = quest.IsCompletedAt(utcNow),
                LastCompletedAtUtc = quest.LastCompletedAtUtc,
            }),
        ];

        return Result.Success(responses);
    }
}
