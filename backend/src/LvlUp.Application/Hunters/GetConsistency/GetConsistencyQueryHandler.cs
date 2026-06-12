using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Hunters.GetConsistency;

internal sealed class GetConsistencyQueryHandler(IApplicationDbContext context, TimeProvider timeProvider)
    : IQueryHandler<GetConsistencyQuery, ConsistencyResponse>
{
    private const int HeatmapDays = 84;
    private const int DisciplineWindowDays = 30;

    public async Task<Result<ConsistencyResponse>> HandleAsync(
        GetConsistencyQuery query,
        CancellationToken cancellationToken)
    {
        Hunter? hunter = await context.Hunters
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == query.HunterId, cancellationToken);

        if (hunter is null)
        {
            return Result.Failure<ConsistencyResponse>(HunterErrors.NotFound(query.HunterId));
        }

        List<DateTime> completionTimes = await context.QuestCompletions
            .AsNoTracking()
            .Where(completion => completion.HunterId == query.HunterId)
            .Select(completion => completion.CompletedAtUtc)
            .ToListAsync(cancellationToken);

        DateOnly today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var completionsPerDay = completionTimes
            .GroupBy(DateOnly.FromDateTime)
            .ToDictionary(group => group.Key, group => group.Count());

        HashSet<DateOnly> activeDays = [.. completionsPerDay.Keys];

        ConsistencyResult result = ConsistencyCalculator.Calculate(activeDays, today);

        DateOnly accountStart = DateOnly.FromDateTime(hunter.CreatedAtUtc);
        int windowDays = Math.Min(DisciplineWindowDays, Math.Max(1, today.DayNumber - accountStart.DayNumber + 1));
        int activeDaysInWindow = activeDays.Count(day => day > today.AddDays(-windowDays) && day <= today);
        int disciplineScore = (int)Math.Round(activeDaysInWindow * 100.0 / windowDays);

        IReadOnlyList<ConsistencyDay> days =
        [
            .. Enumerable.Range(0, HeatmapDays)
                .Select(offset => today.AddDays(offset - (HeatmapDays - 1)))
                .Select(date => new ConsistencyDay(
                    date,
                    completionsPerDay.GetValueOrDefault(date),
                    result.ShieldedDays.Contains(date))),
        ];

        return new ConsistencyResponse
        {
            CurrentStreak = result.CurrentStreak,
            LongestStreak = result.LongestStreak,
            Shields = result.Shields,
            DisciplineScore = disciplineScore,
            Days = days,
        };
    }
}
