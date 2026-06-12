namespace LvlUp.Application.Hunters.GetConsistency;

public sealed record ConsistencyDay(DateOnly Date, int Completions, bool Shielded);

public sealed record ConsistencyResponse
{
    public required int CurrentStreak { get; init; }

    public required int LongestStreak { get; init; }

    public required int Shields { get; init; }

    /// <summary>Percentage (0-100) of days with at least one completion over the last 30 days.</summary>
    public required int DisciplineScore { get; init; }

    /// <summary>The last 84 days (12 weeks), oldest first, for the heatmap.</summary>
    public required IReadOnlyList<ConsistencyDay> Days { get; init; }
}
