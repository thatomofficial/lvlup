using LvlUp.SharedKernel;

namespace LvlUp.Domain.Hunters;

public sealed record HunterLeveledUpDomainEvent(Guid HunterId, int NewLevel) : IDomainEvent;
