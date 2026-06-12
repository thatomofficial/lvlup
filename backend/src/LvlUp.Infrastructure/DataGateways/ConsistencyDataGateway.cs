using LvlUp.Application.Hunters.GetConsistency;
using LvlUp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Infrastructure.DataGateways;

internal sealed class ConsistencyDataGateway(ApplicationDbContext dbContext) : IConsistencyDataGateway
{
    public async Task<IReadOnlyList<DailyCompletionRow>> GetDailyCompletionCountsAsync(
        Guid hunterId,
        CancellationToken cancellationToken = default)
    {
        // Aggregates in the database: one row per active UTC day instead of
        // one row per completion. The streak history grows unbounded, so this
        // keeps the consistency read cheap regardless of how long the hunter
        // has been grinding.
        FormattableString sql = $"""
            SELECT (qc.completed_at_utc AT TIME ZONE 'UTC')::date AS "Day",
                   COUNT(*)::int AS "Completions"
            FROM lvlup.quest_completions qc
            WHERE qc.hunter_id = {hunterId}
            GROUP BY (qc.completed_at_utc AT TIME ZONE 'UTC')::date
            ORDER BY "Day"
            """;

        return await dbContext.Database
            .SqlQuery<DailyCompletionRow>(sql)
            .ToListAsync(cancellationToken);
    }
}
