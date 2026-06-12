using LvlUp.Application.Hunters.GetBadges;
using LvlUp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Infrastructure.DataGateways;

internal sealed class BadgeProgressDataGateway(ApplicationDbContext dbContext) : IBadgeProgressDataGateway
{
    public async Task<IReadOnlyList<CategoryCompletionRow>> GetCompletionCountsByCategoryAsync(
        Guid hunterId,
        CancellationToken cancellationToken = default)
    {
        FormattableString sql = $"""
            SELECT qc.category AS "Category",
                   COUNT(*)::int AS "Completions"
            FROM lvlup.quest_completions qc
            WHERE qc.hunter_id = {hunterId}
            GROUP BY qc.category
            """;

        return await dbContext.Database
            .SqlQuery<CategoryCompletionRow>(sql)
            .ToListAsync(cancellationToken);
    }
}
