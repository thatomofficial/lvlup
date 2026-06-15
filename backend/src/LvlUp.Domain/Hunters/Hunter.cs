using LvlUp.SharedKernel;

namespace LvlUp.Domain.Hunters;

public sealed class Hunter : Entity
{
    public const int BaseStatValue = 10;
    public const int XpPerLevelFactor = 100;
    public const int MinAssessmentScore = 1;
    public const int MaxAssessmentScore = 5;
    public const int MaxPasswordResetAttempts = 5;

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

    public HunterStats Stats { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? AssessedAtUtc { get; private set; }

    public string? GitHubUsername { get; private set; }

    /// <summary>Storage-relative path of the avatar image; the file itself lives in external storage.</summary>
    public string? AvatarPath { get; private set; }

    /// <summary>Hash of the active password-reset code; null when no reset is in progress.</summary>
    public string? PasswordResetCodeHash { get; private set; }

    public DateTime? PasswordResetCodeExpiresAtUtc { get; private set; }

    public int PasswordResetFailedAttempts { get; private set; }

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
        Stats = HunterStats.CreateBase(),
        CreatedAtUtc = utcNow,
    };

    public int GetStat(StatCategory category) => Stats.Get(category);

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

            Stats.Set(category, StatValueForScore(score));
        }

        AssessedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>Maps a 1-5 self-assessment score onto a starting stat of 6-14.</summary>
    public static int StatValueForScore(int score) => 4 + (score * 2);

    public void SetAvatarPath(string? avatarPath) =>
        AvatarPath = string.IsNullOrWhiteSpace(avatarPath) ? null : avatarPath.Trim();

    /// <summary>Stores a freshly issued password-reset code (already hashed) and its expiry.</summary>
    public void RequestPasswordReset(string codeHash, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        PasswordResetCodeHash = codeHash;
        PasswordResetCodeExpiresAtUtc = expiresAtUtc;
        PasswordResetFailedAttempts = 0;
    }

    public bool IsPasswordResetActive(DateTime utcNow) =>
        PasswordResetCodeHash is not null &&
        PasswordResetCodeExpiresAtUtc is { } expiry &&
        expiry > utcNow;

    public void RecordFailedPasswordResetAttempt() => PasswordResetFailedAttempts++;

    public bool HasExhaustedPasswordResetAttempts() =>
        PasswordResetFailedAttempts >= MaxPasswordResetAttempts;

    public void ClearPasswordReset()
    {
        PasswordResetCodeHash = null;
        PasswordResetCodeExpiresAtUtc = null;
        PasswordResetFailedAttempts = 0;
    }

    /// <summary>Sets a new password (already hashed) and clears the reset state.</summary>
    public void CompletePasswordReset(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);

        PasswordHash = newPasswordHash;
        ClearPasswordReset();
    }

    public void SetGitHubUsername(string? username) =>
        GitHubUsername = string.IsNullOrWhiteSpace(username) ? null : username.Trim();

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

    public void IncreaseStat(StatCategory category, int amount) => Stats.Increase(category, amount);
}
