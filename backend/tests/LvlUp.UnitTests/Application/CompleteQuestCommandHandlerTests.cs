using LvlUp.Application.Quests.CompleteQuest;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class CompleteQuestCommandHandlerTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 11, 8, 0, 0, TimeSpan.Zero);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly FixedTimeProvider _timeProvider = new(UtcNow);
    private readonly CompleteQuestCommandHandler _handler;

    public CompleteQuestCommandHandlerTests() => _handler = new CompleteQuestCommandHandler(_context, _timeProvider);

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenQuestDoesNotExist()
    {
        Hunter hunter = await SeedHunterAsync();
        var command = new CompleteQuestCommand(hunter.Id, Guid.NewGuid());

        Result<CompleteQuestResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Error.Code.ShouldBe("Quests.NotFound");
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenQuestBelongsToAnotherHunter()
    {
        Hunter hunter = await SeedHunterAsync();
        Quest foreignQuest = await SeedQuestAsync(Guid.NewGuid(), QuestDifficulty.Easy, QuestType.Daily);
        var command = new CompleteQuestCommand(hunter.Id, foreignQuest.Id);

        Result<CompleteQuestResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_Should_AwardXp_WhenQuestIsCompleted()
    {
        Hunter hunter = await SeedHunterAsync();
        Quest quest = await SeedQuestAsync(hunter.Id, QuestDifficulty.Medium, QuestType.Daily);
        var command = new CompleteQuestCommand(hunter.Id, quest.Id);

        Result<CompleteQuestResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Value.XpGained.ShouldBe(25);
    }

    [Fact]
    public async Task HandleAsync_Should_IncreaseHunterStat_WhenQuestIsCompleted()
    {
        Hunter hunter = await SeedHunterAsync();
        Quest quest = await SeedQuestAsync(hunter.Id, QuestDifficulty.Medium, QuestType.Daily);
        var command = new CompleteQuestCommand(hunter.Id, quest.Id);

        await _handler.HandleAsync(command, CancellationToken.None);

        hunter.Stamina.ShouldBe(Hunter.BaseStatValue + 2);
    }

    [Fact]
    public async Task HandleAsync_Should_RecordQuestCompletion()
    {
        Hunter hunter = await SeedHunterAsync();
        Quest quest = await SeedQuestAsync(hunter.Id, QuestDifficulty.Easy, QuestType.Daily);
        var command = new CompleteQuestCommand(hunter.Id, quest.Id);

        await _handler.HandleAsync(command, CancellationToken.None);

        _context.QuestCompletions.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task HandleAsync_Should_ReportLevelUp_WhenXpThresholdIsCrossed()
    {
        Hunter hunter = await SeedHunterAsync();
        Quest quest = await SeedQuestAsync(hunter.Id, QuestDifficulty.Elite, QuestType.Daily);
        var command = new CompleteQuestCommand(hunter.Id, quest.Id);

        Result<CompleteQuestResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Value.LeveledUp.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_WhenDailyQuestIsAlreadyCompletedToday()
    {
        Hunter hunter = await SeedHunterAsync();
        Quest quest = await SeedQuestAsync(hunter.Id, QuestDifficulty.Easy, QuestType.Daily);
        var command = new CompleteQuestCommand(hunter.Id, quest.Id);
        await _handler.HandleAsync(command, CancellationToken.None);

        Result<CompleteQuestResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Error.Code.ShouldBe("Quests.AlreadyCompleted");
    }

    [Fact]
    public async Task HandleAsync_Should_AllowCompletingDailyQuestAgain_OnTheNextDay()
    {
        Hunter hunter = await SeedHunterAsync();
        Quest quest = await SeedQuestAsync(hunter.Id, QuestDifficulty.Easy, QuestType.Daily);
        var command = new CompleteQuestCommand(hunter.Id, quest.Id);
        await _handler.HandleAsync(command, CancellationToken.None);

        _timeProvider.Advance(TimeSpan.FromDays(1));
        Result<CompleteQuestResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    public void Dispose() => _context.Dispose();

    private async Task<Hunter> SeedHunterAsync()
    {
        var hunter = Hunter.Create("hunter@lvlup.app", "hash", "Jin-Woo", "Sung", "shadow_monarch", UtcNow.UtcDateTime);
        _context.Hunters.Add(hunter);
        await _context.SaveChangesAsync();
        return hunter;
    }

    private async Task<Quest> SeedQuestAsync(Guid hunterId, QuestDifficulty difficulty, QuestType type)
    {
        var quest = Quest.Create(hunterId, "Train", null, StatCategory.Stamina, difficulty, type, UtcNow.UtcDateTime);
        _context.Quests.Add(quest);
        await _context.SaveChangesAsync();
        return quest;
    }
}
