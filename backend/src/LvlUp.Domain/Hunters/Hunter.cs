using LvlUp.SharedKernel;

namespace LvlUp.Domain.Hunters;

public sealed class Hunter : Entity
{
    public const int BaseStatValue = 10;
    public const int XpPerLevelFactor = 100;

    private Hunter()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Surname { get; private set; } = string.Empty;

    public string Username { get; private set; } = string.Empty;

    public DisplayNamePreference DisplayNamePreference { get; private set; }

    public int Level { get; private set; }

    public int CurrentXp { get; private set; }

    public int TotalXp { get; private set; }

    public int Strength { get; private set; }

    public int Stamina { get; private set; }

    public int Physique { get; private set; }

    public int Looks { get; private set; }

    public int WellBeing { get; private set; }

    public int Intelligence { get; private set; }

    public int Charisma { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public int XpForNextLevel => Level * XpPerLevelFactor;

    public string DisplayName => DisplayNamePreference == DisplayNamePreference.Username
        ? Username
        : $"{Name} {Surname}".Trim();

    public HunterRank Rank => Level switch
    {
        >= 50 => HunterRank.S,
        >= 40 => HunterRank.A,
        >= 30 => HunterRank.B,
        >= 20 => HunterRank.C,
        >= 10 => HunterRank.D,
        _ => HunterRank.E,
    };

    public static Hunter Create(
        string email,
        string passwordHash,
        string name,
        string surname,
        string username,
        DateTime utcNow) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = passwordHash,
        Name = name,
        Surname = surname,
        Username = username,
        DisplayNamePreference = DisplayNamePreference.FullName,
        Level = 1,
        CurrentXp = 0,
        TotalXp = 0,
        Strength = BaseStatValue,
        Stamina = BaseStatValue,
        Physique = BaseStatValue,
        Looks = BaseStatValue,
        WellBeing = BaseStatValue,
        Intelligence = BaseStatValue,
        Charisma = BaseStatValue,
        CreatedAtUtc = utcNow,
    };

    public void SetDisplayNamePreference(DisplayNamePreference preference)
    {
        if (!Enum.IsDefined(preference))
        {
            throw new ArgumentOutOfRangeException(nameof(preference), preference, "Unknown display name preference.");
        }

        DisplayNamePreference = preference;
    }

    public bool GainXp(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        CurrentXp += amount;
        TotalXp += amount;

        int startingLevel = Level;
        while (CurrentXp >= XpForNextLevel)
        {
            CurrentXp -= XpForNextLevel;
            Level++;
        }

        bool leveledUp = Level > startingLevel;
        if (leveledUp)
        {
            Raise(new HunterLeveledUpDomainEvent(Id, Level));
        }

        return leveledUp;
    }

    public void IncreaseStat(StatCategory category, int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        switch (category)
        {
            case StatCategory.Strength:
                Strength += amount;
                break;
            case StatCategory.Stamina:
                Stamina += amount;
                break;
            case StatCategory.Physique:
                Physique += amount;
                break;
            case StatCategory.Looks:
                Looks += amount;
                break;
            case StatCategory.WellBeing:
                WellBeing += amount;
                break;
            case StatCategory.Intelligence:
                Intelligence += amount;
                break;
            case StatCategory.Charisma:
                Charisma += amount;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown stat category.");
        }
    }
}
