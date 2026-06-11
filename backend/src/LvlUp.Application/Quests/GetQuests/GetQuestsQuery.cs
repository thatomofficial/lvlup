using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Quests.GetQuests;

public sealed record GetQuestsQuery(Guid HunterId) : IQuery<IReadOnlyList<QuestResponse>>;
