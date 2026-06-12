using LvlUp.Domain.Hunters;

namespace LvlUp.Application.Hunters.GetBadges;

public sealed record BadgeResponse
{
    public required StatCategory Category { get; init; }

    public required BadgeTier Tier { get; init; }

    public required string Name { get; init; }

    public required int RequiredCompletions { get; init; }

    public required int CompletionsInCategory { get; init; }

    public required bool IsEarned { get; init; }

    /// <summary>Progress toward this badge, 0-100.</summary>
    public required int ProgressPercent { get; init; }
}
