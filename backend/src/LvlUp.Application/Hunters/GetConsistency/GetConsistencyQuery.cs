using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Hunters.GetConsistency;

public sealed record GetConsistencyQuery(Guid HunterId) : IQuery<ConsistencyResponse>;
