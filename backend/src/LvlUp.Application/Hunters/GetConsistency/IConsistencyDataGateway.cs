namespace LvlUp.Application.Hunters.GetConsistency;

/// <summary>
/// Reads a hunter's per-day quest completion counts in a single aggregated
/// round trip. Implemented in Infrastructure with a raw SQL GROUP BY query.
/// </summary>
public interface IConsistencyDataGateway
{
    Task<IReadOnlyList<DailyCompletionRow>> GetDailyCompletionCountsAsync(
        Guid hunterId,
        CancellationToken cancellationToken = default);
}
