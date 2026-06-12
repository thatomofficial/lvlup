namespace LvlUp.Application.Hunters.GetBadges;

public sealed record CategoryCompletionRow
{
    public int Category { get; init; }

    public int Completions { get; init; }
}
