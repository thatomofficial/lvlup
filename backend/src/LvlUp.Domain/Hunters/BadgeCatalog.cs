namespace LvlUp.Domain.Hunters;

/// <summary>
/// The static badge catalog: every stat category has a themed badge line
/// with four tiers, earned by completing quests of that category.
/// </summary>
public static class BadgeCatalog
{
    private static readonly Dictionary<StatCategory, string> CategoryTitles =
        new()
        {
            [StatCategory.Strength] = "Titan's Grip",
            [StatCategory.Stamina] = "Tireless Runner",
            [StatCategory.Physique] = "Tempered Body",
            [StatCategory.Looks] = "Radiant Aura",
            [StatCategory.WellBeing] = "Inner Light",
            [StatCategory.Intelligence] = "Arcane Scholar",
            [StatCategory.Charisma] = "Silver Tongue",
        };

    private static readonly (BadgeTier Tier, int RequiredCompletions, string Numeral)[] Tiers =
    [
        (BadgeTier.Iron, 5, "I"),
        (BadgeTier.Steel, 25, "II"),
        (BadgeTier.Mythril, 75, "III"),
        (BadgeTier.Monarch, 200, "IV"),
    ];

    public static IReadOnlyList<BadgeDefinition> All { get; } =
    [
        .. Enum.GetValues<StatCategory>()
            .SelectMany(category => Tiers.Select(tier => new BadgeDefinition(
                category,
                tier.Tier,
                $"{CategoryTitles[category]} {tier.Numeral}",
                tier.RequiredCompletions))),
    ];
}
