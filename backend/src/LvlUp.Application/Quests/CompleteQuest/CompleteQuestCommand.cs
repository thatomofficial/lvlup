using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Quests.CompleteQuest;

public sealed record CompleteQuestCommand(Guid HunterId, Guid QuestId) : ICommand<CompleteQuestResponse>;
