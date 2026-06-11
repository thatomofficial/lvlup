using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using Shouldly;

namespace LvlUp.UnitTests.Domain;

public class QuestTests
{
    private static readonly DateTime UtcNow = new(2026, 6, 11, 8, 0, 0, DateTimeKind.Utc);

    private static Quest CreateQuest(
        QuestDifficulty difficulty = QuestDifficulty.Medium,
        QuestType type = QuestType.Daily) =>
        Quest.Create(Guid.NewGuid(), "Morning run", "5km outside", StatCategory.Stamina, difficulty, type, UtcNow);

    [Theory]
    [InlineData(QuestDifficulty.Easy, 10)]
    [InlineData(QuestDifficulty.Medium, 25)]
    [InlineData(QuestDifficulty.Hard, 50)]
    [InlineData(QuestDifficulty.Elite, 100)]
    public void XpReward_Should_ScaleWithDifficulty(QuestDifficulty difficulty, int expectedXp)
    {
        Quest quest = CreateQuest(difficulty);

        quest.XpReward.ShouldBe(expectedXp);
    }

    [Theory]
    [InlineData(QuestDifficulty.Easy, 1)]
    [InlineData(QuestDifficulty.Medium, 2)]
    [InlineData(QuestDifficulty.Hard, 3)]
    [InlineData(QuestDifficulty.Elite, 5)]
    public void StatReward_Should_ScaleWithDifficulty(QuestDifficulty difficulty, int expectedStat)
    {
        Quest quest = CreateQuest(difficulty);

        quest.StatReward.ShouldBe(expectedStat);
    }

    [Fact]
    public void Complete_Should_Succeed_WhenQuestHasNeverBeenCompleted()
    {
        Quest quest = CreateQuest();

        Result result = quest.Complete(UtcNow);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Complete_Should_RaiseQuestCompletedDomainEvent()
    {
        Quest quest = CreateQuest();

        quest.Complete(UtcNow);

        quest.DomainEvents.ShouldContain(domainEvent => domainEvent is QuestCompletedDomainEvent);
    }

    [Fact]
    public void Complete_Should_Fail_WhenOneTimeQuestIsAlreadyCompleted()
    {
        Quest quest = CreateQuest(type: QuestType.OneTime);
        quest.Complete(UtcNow);

        Result result = quest.Complete(UtcNow.AddDays(3));

        result.Error.Code.ShouldBe("Quests.AlreadyCompleted");
    }

    [Fact]
    public void Complete_Should_Fail_WhenDailyQuestIsAlreadyCompletedToday()
    {
        Quest quest = CreateQuest(type: QuestType.Daily);
        quest.Complete(UtcNow);

        Result result = quest.Complete(UtcNow.AddHours(2));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Complete_Should_Succeed_WhenDailyQuestWasCompletedOnAPreviousDay()
    {
        Quest quest = CreateQuest(type: QuestType.Daily);
        quest.Complete(UtcNow);

        Result result = quest.Complete(UtcNow.AddDays(1));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void IsCompletedAt_Should_BeFalse_ForDailyQuest_OnTheNextDay()
    {
        Quest quest = CreateQuest(type: QuestType.Daily);
        quest.Complete(UtcNow);

        quest.IsCompletedAt(UtcNow.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    public void IsCompletedAt_Should_BeTrue_ForOneTimeQuest_OnAnyLaterDay()
    {
        Quest quest = CreateQuest(type: QuestType.OneTime);
        quest.Complete(UtcNow);

        quest.IsCompletedAt(UtcNow.AddDays(30)).ShouldBeTrue();
    }
}
