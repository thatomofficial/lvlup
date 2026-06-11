using LvlUp.Domain.Hunters;

namespace LvlUp.Application.Quests.CompleteQuest;

public sealed record CompleteQuestResponse
{
    public required int XpGained { get; init; }

    public required StatCategory StatCategory { get; init; }

    public required int StatGained { get; init; }

    public required bool LeveledUp { get; init; }

    public required int NewLevel { get; init; }

    public required int CurrentXp { get; init; }

    public required int XpForNextLevel { get; init; }
}
