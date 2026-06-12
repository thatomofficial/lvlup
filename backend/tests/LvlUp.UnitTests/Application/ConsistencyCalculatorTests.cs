using LvlUp.Application.Hunters.GetConsistency;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public class ConsistencyCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 6, 11);

    [Fact]
    public void Calculate_Should_ReturnZeros_WhenThereIsNoActivity()
    {
        ConsistencyResult result = ConsistencyCalculator.Calculate(new HashSet<DateOnly>(), Today);

        result.ShouldSatisfyAllConditions(
            r => r.CurrentStreak.ShouldBe(0),
            r => r.LongestStreak.ShouldBe(0),
            r => r.Shields.ShouldBe(0));
    }

    [Fact]
    public void Calculate_Should_CountConsecutiveActiveDays()
    {
        HashSet<DateOnly> days = ActiveDays(Today, 5);

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.CurrentStreak.ShouldBe(5);
    }

    [Fact]
    public void Calculate_Should_NotBreakStreak_WhenTodayIsStillPending()
    {
        // Active the previous 4 days but nothing logged today yet.
        HashSet<DateOnly> days = ActiveDays(Today.AddDays(-1), 4);

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.CurrentStreak.ShouldBe(4);
    }

    [Fact]
    public void Calculate_Should_ResetStreak_OnMissedDayWithoutShield()
    {
        // 3 active days, a gap, then 2 active days ending today.
        HashSet<DateOnly> days = [.. ActiveDays(Today.AddDays(-6), 3), .. ActiveDays(Today, 2)];

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.CurrentStreak.ShouldBe(2);
    }

    [Fact]
    public void Calculate_Should_TrackLongestStreakAcrossResets()
    {
        HashSet<DateOnly> days = [.. ActiveDays(Today.AddDays(-10), 6), .. ActiveDays(Today, 2)];

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.LongestStreak.ShouldBe(6);
    }

    [Fact]
    public void Calculate_Should_EarnShield_AfterSevenConsecutiveActiveDays()
    {
        HashSet<DateOnly> days = ActiveDays(Today, 7);

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.Shields.ShouldBe(1);
    }

    [Fact]
    public void Calculate_Should_ConsumeShield_ToPreserveStreakOnMissedDay()
    {
        // 7 active days earn a shield, one missed day consumes it, 2 more active days.
        HashSet<DateOnly> days = [.. ActiveDays(Today.AddDays(-3), 7), .. ActiveDays(Today, 2)];

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.CurrentStreak.ShouldBe(9);
    }

    [Fact]
    public void Calculate_Should_MarkShieldedDays()
    {
        HashSet<DateOnly> days = [.. ActiveDays(Today.AddDays(-3), 7), .. ActiveDays(Today, 2)];

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.ShieldedDays.ShouldContain(Today.AddDays(-2));
    }

    [Fact]
    public void Calculate_Should_ResetStreak_WhenMissesExceedShields()
    {
        // One shield earned, but two consecutive missed days.
        HashSet<DateOnly> days = [.. ActiveDays(Today.AddDays(-4), 7), .. ActiveDays(Today, 2)];

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.CurrentStreak.ShouldBe(2);
    }

    [Fact]
    public void Calculate_Should_CapShieldsAtThree()
    {
        // 35 consecutive active days would earn 5 shields without the cap.
        HashSet<DateOnly> days = ActiveDays(Today, 35);

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.Shields.ShouldBe(3);
    }

    [Fact]
    public void Calculate_Should_RequireFreshSevenDayRun_AfterShieldConsumption()
    {
        // 7 actives earn a shield; miss consumes it; only 5 actives since - no new shield.
        HashSet<DateOnly> days = [.. ActiveDays(Today.AddDays(-6), 7), .. ActiveDays(Today, 5)];

        ConsistencyResult result = ConsistencyCalculator.Calculate(days, Today);

        result.Shields.ShouldBe(0);
    }

    /// <summary>Builds a run of consecutive active days ending on <paramref name="lastDay"/>.</summary>
    private static HashSet<DateOnly> ActiveDays(DateOnly lastDay, int count) =>
        [.. Enumerable.Range(0, count).Select(offset => lastDay.AddDays(-offset))];
}
