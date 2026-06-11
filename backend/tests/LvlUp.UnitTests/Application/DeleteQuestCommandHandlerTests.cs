using LvlUp.Application.Quests.DeleteQuest;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class DeleteQuestCommandHandlerTests : IDisposable
{
    private static readonly DateTime UtcNow = new(2026, 6, 11, 8, 0, 0, DateTimeKind.Utc);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly DeleteQuestCommandHandler _handler;

    public DeleteQuestCommandHandlerTests() => _handler = new DeleteQuestCommandHandler(_context);

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenQuestDoesNotExist()
    {
        var command = new DeleteQuestCommand(Guid.NewGuid(), Guid.NewGuid());

        Result result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Error.Code.ShouldBe("Quests.NotFound");
    }

    [Fact]
    public async Task HandleAsync_Should_RemoveQuest_WhenQuestExists()
    {
        Quest quest = await SeedQuestAsync();
        var command = new DeleteQuestCommand(quest.HunterId, quest.Id);

        await _handler.HandleAsync(command, CancellationToken.None);

        _context.Quests.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenQuestBelongsToAnotherHunter()
    {
        Quest quest = await SeedQuestAsync();
        var command = new DeleteQuestCommand(Guid.NewGuid(), quest.Id);

        Result result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
    }

    public void Dispose() => _context.Dispose();

    private async Task<Quest> SeedQuestAsync()
    {
        var quest = Quest.Create(
            Guid.NewGuid(),
            "Train",
            null,
            StatCategory.Strength,
            QuestDifficulty.Easy,
            QuestType.Daily,
            UtcNow);

        _context.Quests.Add(quest);
        await _context.SaveChangesAsync();

        return quest;
    }
}
