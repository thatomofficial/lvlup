using LvlUp.Domain.Hunters;
using Shouldly;

namespace LvlUp.UnitTests.Domain;

public class HunterTests
{
    private static readonly DateTime UtcNow = new(2026, 6, 11, 8, 0, 0, DateTimeKind.Utc);

    private static Hunter CreateHunter() =>
        Hunter.Create("hunter@lvlup.app", "hash", "Jin-Woo", "Sung", "shadow_monarch", UtcNow);

    [Fact]
    public void Create_Should_StartAtLevelOne()
    {
        Hunter hunter = CreateHunter();

        hunter.Level.ShouldBe(1);
    }

    [Fact]
    public void Create_Should_StartWithBaseStats()
    {
        Hunter hunter = CreateHunter();

        hunter.ShouldSatisfyAllConditions(
            h => h.Strength.ShouldBe(Hunter.BaseStatValue),
            h => h.Stamina.ShouldBe(Hunter.BaseStatValue),
            h => h.Physique.ShouldBe(Hunter.BaseStatValue),
            h => h.Looks.ShouldBe(Hunter.BaseStatValue),
            h => h.WellBeing.ShouldBe(Hunter.BaseStatValue));
    }

    [Fact]
    public void Create_Should_StartAtRankE()
    {
        Hunter hunter = CreateHunter();

        hunter.Rank.ShouldBe(HunterRank.E);
    }

    [Fact]
    public void DisplayName_Should_BeFullName_ByDefault()
    {
        Hunter hunter = CreateHunter();

        hunter.DisplayName.ShouldBe("Jin-Woo Sung");
    }

    [Fact]
    public void DisplayName_Should_BeUsername_WhenPreferenceIsUsername()
    {
        Hunter hunter = CreateHunter();

        hunter.SetDisplayNamePreference(DisplayNamePreference.Username);

        hunter.DisplayName.ShouldBe("shadow_monarch");
    }

    [Fact]
    public void SetDisplayNamePreference_Should_Throw_WhenPreferenceIsUnknown()
    {
        Hunter hunter = CreateHunter();

        Should.Throw<ArgumentOutOfRangeException>(() =>
            hunter.SetDisplayNamePreference((DisplayNamePreference)99));
    }

    [Fact]
    public void GainXp_Should_NotLevelUp_WhenXpIsBelowThreshold()
    {
        Hunter hunter = CreateHunter();

        bool leveledUp = hunter.GainXp(99);

        leveledUp.ShouldBeFalse();
    }

    [Fact]
    public void GainXp_Should_LevelUp_WhenXpReachesThreshold()
    {
        Hunter hunter = CreateHunter();

        hunter.GainXp(100);

        hunter.Level.ShouldBe(2);
    }

    [Fact]
    public void GainXp_Should_CarryOverExcessXp()
    {
        Hunter hunter = CreateHunter();

        hunter.GainXp(150);

        hunter.CurrentXp.ShouldBe(50);
    }

    [Fact]
    public void GainXp_Should_LevelUpMultipleTimes_WhenXpCoversMultipleLevels()
    {
        Hunter hunter = CreateHunter();

        // Level 1 -> 2 costs 100 XP and level 2 -> 3 costs 200 XP.
        hunter.GainXp(300);

        hunter.Level.ShouldBe(3);
    }

    [Fact]
    public void GainXp_Should_TrackTotalXp()
    {
        Hunter hunter = CreateHunter();

        hunter.GainXp(150);

        hunter.TotalXp.ShouldBe(150);
    }

    [Fact]
    public void GainXp_Should_RaiseHunterLeveledUpDomainEvent_WhenLevelingUp()
    {
        Hunter hunter = CreateHunter();

        hunter.GainXp(100);

        hunter.DomainEvents.ShouldContain(domainEvent =>
            domainEvent is HunterLeveledUpDomainEvent && ((HunterLeveledUpDomainEvent)domainEvent).NewLevel == 2);
    }

    [Fact]
    public void GainXp_Should_NotRaiseDomainEvent_WhenNotLevelingUp()
    {
        Hunter hunter = CreateHunter();

        hunter.GainXp(50);

        hunter.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void GainXp_Should_Throw_WhenAmountIsNotPositive()
    {
        Hunter hunter = CreateHunter();

        Should.Throw<ArgumentOutOfRangeException>(() => hunter.GainXp(0));
    }

    [Theory]
    [InlineData(StatCategory.Strength)]
    [InlineData(StatCategory.Stamina)]
    [InlineData(StatCategory.Physique)]
    [InlineData(StatCategory.Looks)]
    [InlineData(StatCategory.WellBeing)]
    public void IncreaseStat_Should_IncreaseTheTargetedStat(StatCategory category)
    {
        Hunter hunter = CreateHunter();

        hunter.IncreaseStat(category, 3);

        GetStat(hunter, category).ShouldBe(Hunter.BaseStatValue + 3);
    }

    [Fact]
    public void IncreaseStat_Should_Throw_WhenAmountIsNotPositive()
    {
        Hunter hunter = CreateHunter();

        Should.Throw<ArgumentOutOfRangeException>(() => hunter.IncreaseStat(StatCategory.Strength, -1));
    }

    [Theory]
    [InlineData(1, HunterRank.E)]
    [InlineData(10, HunterRank.D)]
    [InlineData(20, HunterRank.C)]
    [InlineData(30, HunterRank.B)]
    [InlineData(40, HunterRank.A)]
    [InlineData(50, HunterRank.S)]
    public void Rank_Should_MatchLevelThreshold(int targetLevel, HunterRank expectedRank)
    {
        Hunter hunter = CreateHunter();

        // Total XP required to reach level n from level 1 is 100 * (1 + 2 + ... + (n - 1)).
        int requiredXp = 100 * (targetLevel * (targetLevel - 1) / 2);
        if (requiredXp > 0)
        {
            hunter.GainXp(requiredXp);
        }

        hunter.Rank.ShouldBe(expectedRank);
    }

    private static int GetStat(Hunter hunter, StatCategory category) => category switch
    {
        StatCategory.Strength => hunter.Strength,
        StatCategory.Stamina => hunter.Stamina,
        StatCategory.Physique => hunter.Physique,
        StatCategory.Looks => hunter.Looks,
        StatCategory.WellBeing => hunter.WellBeing,
        _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };
}
