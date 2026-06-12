using LvlUp.Application.Quests.CreateStarterPack;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class CreateStarterPackCommandHandlerTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 11, 8, 0, 0, TimeSpan.Zero);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly CreateStarterPackCommandHandler _handler;

    public CreateStarterPackCommandHandlerTests() =>
        _handler = new CreateStarterPackCommandHandler(_context, new FixedTimeProvider(UtcNow));

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenHunterDoesNotExist()
    {
        var command = new CreateStarterPackCommand(Guid.NewGuid());

        Result<CreateStarterPackResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Error.Code.ShouldBe("Hunters.NotFound");
    }

    [Fact]
    public async Task HandleAsync_Should_CreateAllStarterQuests_ForNewHunter()
    {
        Hunter hunter = await SeedHunterAsync();

        Result<CreateStarterPackResponse> result = await _handler.HandleAsync(
            new CreateStarterPackCommand(hunter.Id),
            CancellationToken.None);

        result.Value.CreatedCount.ShouldBe(14);
    }

    [Fact]
    public async Task HandleAsync_Should_SkipExistingQuests_OnSecondRun()
    {
        Hunter hunter = await SeedHunterAsync();
        await _handler.HandleAsync(new CreateStarterPackCommand(hunter.Id), CancellationToken.None);

        Result<CreateStarterPackResponse> result = await _handler.HandleAsync(
            new CreateStarterPackCommand(hunter.Id),
            CancellationToken.None);

        result.Value.CreatedCount.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_Should_CalibrateDailyDifficultyToLowStat()
    {
        // Assessment score 1 -> stat 6 -> Easy quests for that category.
        Hunter hunter = await SeedHunterAsync(charismaScore: 1);

        await _handler.HandleAsync(new CreateStarterPackCommand(hunter.Id), CancellationToken.None);

        Quest conversationQuest = _context.Quests.Single(q => q.Title == "Start one conversation");
        conversationQuest.Difficulty.ShouldBe(QuestDifficulty.Easy);
    }

    [Fact]
    public async Task HandleAsync_Should_CalibrateDailyDifficultyToHighStat()
    {
        // Assessment score 5 -> stat 14 -> Hard quests for that category.
        Hunter hunter = await SeedHunterAsync(strengthScore: 5);

        await _handler.HandleAsync(new CreateStarterPackCommand(hunter.Id), CancellationToken.None);

        Quest workoutQuest = _context.Quests.Single(q => q.Title == "Workout session");
        workoutQuest.Difficulty.ShouldBe(QuestDifficulty.Hard);
    }

    [Fact]
    public async Task HandleAsync_Should_KeepMilestoneQuestsElite_RegardlessOfStats()
    {
        Hunter hunter = await SeedHunterAsync(charismaScore: 1);

        await _handler.HandleAsync(new CreateStarterPackCommand(hunter.Id), CancellationToken.None);

        Quest milestone = _context.Quests.Single(q => q.Title == "Hold a 5-minute conversation");
        milestone.Difficulty.ShouldBe(QuestDifficulty.Elite);
    }

    public void Dispose() => _context.Dispose();

    private async Task<Hunter> SeedHunterAsync(int strengthScore = 3, int charismaScore = 3)
    {
        var hunter = Hunter.Create("hunter@lvlup.app", "hash", "Jin-Woo", "Sung", "shadow_monarch", UtcNow.UtcDateTime);

        hunter.ApplyAssessment(
            new Dictionary<StatCategory, int>
            {
                [StatCategory.Strength] = strengthScore,
                [StatCategory.Stamina] = 3,
                [StatCategory.Physique] = 3,
                [StatCategory.Looks] = 3,
                [StatCategory.WellBeing] = 3,
                [StatCategory.Intelligence] = 3,
                [StatCategory.Charisma] = charismaScore,
            },
            UtcNow.UtcDateTime);

        _context.Hunters.Add(hunter);
        await _context.SaveChangesAsync();

        return hunter;
    }
}
