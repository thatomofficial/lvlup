using LvlUp.Application.Hunters.SubmitAssessment;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class SubmitAssessmentCommandHandlerTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 11, 8, 0, 0, TimeSpan.Zero);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly SubmitAssessmentCommandHandler _handler;

    public SubmitAssessmentCommandHandlerTests() =>
        _handler = new SubmitAssessmentCommandHandler(_context, new FixedTimeProvider(UtcNow));

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenHunterDoesNotExist()
    {
        var command = new SubmitAssessmentCommand(Guid.NewGuid(), CreateScores());

        Result<AssessmentResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Error.Code.ShouldBe("Hunters.NotFound");
    }

    [Fact]
    public async Task HandleAsync_Should_SetStartingStatsFromScores()
    {
        Hunter hunter = await SeedHunterAsync();
        var command = new SubmitAssessmentCommand(hunter.Id, CreateScores(charisma: 1));

        Result<AssessmentResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Value.Stats.Charisma.ShouldBe(6);
    }

    [Fact]
    public async Task HandleAsync_Should_RecommendEasyQuests_ForLowScores()
    {
        Hunter hunter = await SeedHunterAsync();
        var command = new SubmitAssessmentCommand(hunter.Id, CreateScores(charisma: 1));

        Result<AssessmentResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Value.RecommendedDifficulties[StatCategory.Charisma].ShouldBe(QuestDifficulty.Easy);
    }

    [Fact]
    public async Task HandleAsync_Should_RecommendHardQuests_ForHighScores()
    {
        Hunter hunter = await SeedHunterAsync();
        var command = new SubmitAssessmentCommand(hunter.Id, CreateScores(strength: 5));

        Result<AssessmentResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Value.RecommendedDifficulties[StatCategory.Strength].ShouldBe(QuestDifficulty.Hard);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_WhenAssessmentIsRepeated()
    {
        Hunter hunter = await SeedHunterAsync();
        await _handler.HandleAsync(new SubmitAssessmentCommand(hunter.Id, CreateScores()), CancellationToken.None);

        Result<AssessmentResponse> result = await _handler.HandleAsync(
            new SubmitAssessmentCommand(hunter.Id, CreateScores()),
            CancellationToken.None);

        result.Error.ShouldBe(HunterErrors.AlreadyAssessed);
    }

    public void Dispose() => _context.Dispose();

    private static Dictionary<StatCategory, int> CreateScores(int strength = 3, int charisma = 3) => new()
    {
        [StatCategory.Strength] = strength,
        [StatCategory.Stamina] = 3,
        [StatCategory.Physique] = 3,
        [StatCategory.Looks] = 3,
        [StatCategory.WellBeing] = 3,
        [StatCategory.Intelligence] = 3,
        [StatCategory.Charisma] = charisma,
    };

    private async Task<Hunter> SeedHunterAsync()
    {
        var hunter = Hunter.Create("hunter@lvlup.app", "hash", "Jin-Woo", "Sung", "shadow_monarch", UtcNow.UtcDateTime);
        _context.Hunters.Add(hunter);
        await _context.SaveChangesAsync();
        return hunter;
    }
}
