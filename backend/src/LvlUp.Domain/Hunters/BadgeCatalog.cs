namespace LvlUp.Domain.Hunters;

/// <summary>
/// The static badge catalog: every stat category has four distinct badges,
/// earned by completing quests of that category.
/// </summary>
public static class BadgeCatalog
{
    private static readonly Dictionary<StatCategory, string[]> TierNames = new()
    {
        [StatCategory.Strength] = ["Iron Grip", "Steel Breaker", "Titan's Grip", "Monarch of Might"],
        [StatCategory.Stamina] = ["First Wind", "Pacekeeper", "Tireless Runner", "Marathon Monarch"],
        [StatCategory.Physique] = ["Limbered Up", "Tempered Body", "Iron Frame", "Living Fortress"],
        [StatCategory.Looks] = ["Fresh Face", "Well Groomed", "Radiant Aura", "Dazzling Presence"],
        [StatCategory.WellBeing] = ["First Light", "Daily Bread", "Inner Light", "Unshakeable Spirit"],
        [StatCategory.Intelligence] = ["Quick Study", "Page Turner", "Arcane Scholar", "Archmage"],
        [StatCategory.Charisma] = ["Ice Breaker", "Conversationalist", "Silver Tongue", "Monarch's Voice"],
    };

    private static readonly (BadgeTier Tier, int RequiredCompletions)[] Tiers =
    [
        (BadgeTier.Iron, 5),
        (BadgeTier.Steel, 25),
        (BadgeTier.Mythril, 75),
        (BadgeTier.Monarch, 200),
    ];

    public static IReadOnlyList<BadgeDefinition> All { get; } =
    [
        .. Enum.GetValues<StatCategory>()
            .SelectMany(category => Tiers.Select((tier, index) => new BadgeDefinition(
                category,
                tier.Tier,
                TierNames[category][index],
                tier.RequiredCompletions))),
    ];
}
