using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Hunters.GetHunter;

public sealed record GetHunterQuery(Guid HunterId) : IQuery<HunterResponse>;
