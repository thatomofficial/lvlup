using LvlUp.SharedKernel;

namespace LvlUp.Domain.Hunters;

public sealed class Hunter : Entity
{
    public const int BaseStatValue = 10;
    public const int XpPerLevelFactor = 100;
    public const int MinAssessmentScore = 1;
    public const int MaxAssessmentScore = 5;

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

    public DateTime? AssessedAtUtc { get; private set; }

    public bool HasCompletedAssessment => AssessedAtUtc is not null;

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

    public int GetStat(StatCategory category) => category switch
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

    /// <summary>
    /// Applies the one-time awakening self-assessment: each category score (1-5)
    /// sets the starting stat so new hunters begin at their actual level.
    /// </summary>
    public Result ApplyAssessment(IReadOnlyDictionary<StatCategory, int> scores, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(scores);

        if (HasCompletedAssessment)
        {
            return Result.Failure(HunterErrors.AlreadyAssessed);
        }

        if (TotalXp > 0)
        {
            return Result.Failure(HunterErrors.AssessmentUnavailable);
        }

        foreach ((StatCategory category, int score) in scores)
        {
            if (score is < MinAssessmentScore or > MaxAssessmentScore)
            {
                throw new ArgumentOutOfRangeException(nameof(scores), score, "Assessment scores must be between 1 and 5.");
            }

            SetStat(category, StatValueForScore(score));
        }

        AssessedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>Maps a 1-5 self-assessment score onto a starting stat of 6-14.</summary>
    public static int StatValueForScore(int score) => 4 + (score * 2);

    private void SetStat(StatCategory category, int value)
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
