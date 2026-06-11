using LvlUp.SharedKernel;

namespace LvlUp.Domain.Quests;

public sealed record QuestCompletedDomainEvent(Guid QuestId, Guid HunterId) : IDomainEvent;
