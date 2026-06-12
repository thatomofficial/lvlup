namespace LvlUp.Domain.Hunters;

/// <summary>
/// The hunter's seven trainable stats. Persisted in its own table
/// (one row per hunter, keyed by the hunter id).
/// </summary>
public sealed class HunterStats
{
    private HunterStats()
    {
    }

    public int Strength { get; private set; }

    public int Stamina { get; private set; }

    public int Physique { get; private set; }

    public int Looks { get; private set; }

    public int WellBeing { get; private set; }

    public int Intelligence { get; private set; }

    public int Charisma { get; private set; }

    public int Get(StatCategory category) => category switch
    {
        StatCategory.Strength => Strength,
        StatCategory.Stamina => Stamina,
        StatCategory.Physique => Physique,
        StatCategory.Looks => Looks,
        StatCategory.WellBeing => WellBeing,
        StatCategory.Intelligence => Intelligence,
        StatCategory.Charisma => Charisma,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown stat category."),
    };

    internal static HunterStats CreateBase() => new()
    {
        Strength = Hunter.BaseStatValue,
        Stamina = Hunter.BaseStatValue,
        Physique = Hunter.BaseStatValue,
        Looks = Hunter.BaseStatValue,
        WellBeing = Hunter.BaseStatValue,
        Intelligence = Hunter.BaseStatValue,
        Charisma = Hunter.BaseStatValue,
    };

    internal void Increase(StatCategory category, int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        Set(category, Get(category) + amount);
    }

    internal void Set(StatCategory category, int value)
    {
        switch (category)
        {
            case StatCategory.Strength:
                Strength = value;
                break;
            case StatCategory.Stamina:
                Stamina = value;
                break;
            case StatCategory.Physique:
                Physique = value;
                break;
            case StatCategory.Looks:
                Looks = value;
                break;
            case StatCategory.WellBeing:
                WellBeing = value;
                break;
            case StatCategory.Intelligence:
                Intelligence = value;
                break;
            case StatCategory.Charisma:
                Charisma = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown stat category.");
        }
    }
}
