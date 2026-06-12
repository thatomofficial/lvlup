using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Hunters.GetBadges;

public sealed record GetBadgesQuery(Guid HunterId) : IQuery<IReadOnlyList<BadgeResponse>>;
