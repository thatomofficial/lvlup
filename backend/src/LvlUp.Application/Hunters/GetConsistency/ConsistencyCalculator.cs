namespace LvlUp.Application.Hunters.GetConsistency;

internal sealed record ConsistencyResult(
    int CurrentStreak,
    int LongestStreak,
    int Shields,
    IReadOnlySet<DateOnly> ShieldedDays);

/// <summary>
/// Derives streaks and streak shields purely from the set of active days
/// (days with at least one quest completion). No state is persisted: the
/// calculation is deterministic over the completion log.
///
/// Rules:
/// - A day with at least one completion extends the streak.
/// - Every 7 consecutive active days earns one shield (max 3 banked).
/// - A missed day consumes a shield, preserving the streak; with no shields
///   the streak resets. Shield progress toward the next shield also resets.
/// - Today never breaks the streak while it is still in progress.
/// </summary>
internal static class ConsistencyCalculator
{
    public const int ShieldEarnedEveryActiveDays = 7;
    public const int MaxShields = 3;

    public static ConsistencyResult Calculate(IReadOnlySet<DateOnly> activeDays, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(activeDays);

        if (activeDays.Count == 0)
        {
            return new ConsistencyResult(0, 0, 0, new HashSet<DateOnly>());
        }

        DateOnly firstActiveDay = activeDays.Min();

        int currentStreak = 0;
        int longestStreak = 0;
        int shields = 0;
        int progressTowardShield = 0;
        var shieldedDays = new HashSet<DateOnly>();

        for (DateOnly day = firstActiveDay; day <= today; day = day.AddDays(1))
        {
            if (activeDays.Contains(day))
            {
                currentStreak++;
                progressTowardShield++;

                if (progressTowardShield % ShieldEarnedEveryActiveDays == 0 && shields < MaxShields)
                {
                    shields++;
                }

                longestStreak = Math.Max(longestStreak, currentStreak);
            }
            else if (day == today)
            {
                // Today is still in progress: a missing completion is pending, not a miss.
            }
            else if (shields > 0)
            {
                shields--;
                shieldedDays.Add(day);
                progressTowardShield = 0;
            }
            else
            {
                currentStreak = 0;
                progressTowardShield = 0;
            }
        }

        return new ConsistencyResult(currentStreak, longestStreak, shields, shieldedDays);
    }
}
