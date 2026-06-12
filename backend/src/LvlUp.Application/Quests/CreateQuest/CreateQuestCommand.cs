using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;

namespace LvlUp.Application.Quests.CreateQuest;

public sealed record CreateQuestCommand(
    Guid HunterId,
    string Title,
    string? Description,
    StatCategory Category,
    QuestDifficulty Difficulty,
    QuestType Type,
    QuestVerification Verification) : ICommand<Guid>;
