using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Hunters.GetBadges;

internal sealed class GetBadgesQueryHandler(
    IApplicationDbContext context,
    IBadgeProgressDataGateway badgeProgressDataGateway)
    : IQueryHandler<GetBadgesQuery, IReadOnlyList<BadgeResponse>>
{
    public async Task<Result<IReadOnlyList<BadgeResponse>>> HandleAsync(
        GetBadgesQuery query,
        CancellationToken cancellationToken)
    {
        bool hunterExists = await context.Hunters
            .AsNoTracking()
            .AnyAsync(h => h.Id == query.HunterId, cancellationToken);

        if (!hunterExists)
        {
            return Result.Failure<IReadOnlyList<BadgeResponse>>(HunterErrors.NotFound(query.HunterId));
        }

        IReadOnlyList<CategoryCompletionRow> rows =
            await badgeProgressDataGateway.GetCompletionCountsByCategoryAsync(query.HunterId, cancellationToken);

        var completionsByCategory = rows.ToDictionary(
            row => (StatCategory)row.Category,
            row => row.Completions);

        IReadOnlyList<BadgeResponse> badges =
        [
            .. BadgeCatalog.All.Select(definition =>
            {
                int completions = completionsByCategory.GetValueOrDefault(definition.Category);

                return new BadgeResponse
                {
                    Category = definition.Category,
                    Tier = definition.Tier,
                    Name = definition.Name,
                    RequiredCompletions = definition.RequiredCompletions,
                    CompletionsInCategory = completions,
                    IsEarned = completions >= definition.RequiredCompletions,
                    ProgressPercent = (int)Math.Min(100, Math.Round(completions * 100.0 / definition.RequiredCompletions)),
                };
            }),
        ];

        return Result.Success(badges);
    }
}
