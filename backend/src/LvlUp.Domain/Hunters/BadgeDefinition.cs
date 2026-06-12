namespace LvlUp.Domain.Hunters;

public sealed record BadgeDefinition(
    StatCategory Category,
    BadgeTier Tier,
    string Name,
    int RequiredCompletions);
