namespace LvlUp.Application.Hunters.GetConsistency;

public sealed record DailyCompletionRow
{
    public DateOnly Day { get; init; }

    public int Completions { get; init; }
}
