using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;

namespace LvlUp.Application.Quests;

public sealed record QuestResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string? Description { get; init; }

    public required StatCategory Category { get; init; }

    public required QuestDifficulty Difficulty { get; init; }

    public required QuestType Type { get; init; }

    public required int XpReward { get; init; }

    public required int StatReward { get; init; }

    public required bool IsCompleted { get; init; }

    public required DateTime? LastCompletedAtUtc { get; init; }
}
