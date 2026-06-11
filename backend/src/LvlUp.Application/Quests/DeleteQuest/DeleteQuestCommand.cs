using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Quests.DeleteQuest;

public sealed record DeleteQuestCommand(Guid HunterId, Guid QuestId) : ICommand;
