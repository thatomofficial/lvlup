using LvlUp.Application.Abstractions.Events;
using LvlUp.Domain.Hunters;
using Microsoft.Extensions.Logging;

namespace LvlUp.Application.Hunters;

internal sealed class HunterLeveledUpDomainEventHandler(ILogger<HunterLeveledUpDomainEventHandler> logger)
    : IDomainEventHandler<HunterLeveledUpDomainEvent>
{
    public Task HandleAsync(HunterLeveledUpDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Hunter {HunterId} has leveled up to level {NewLevel}",
            domainEvent.HunterId,
            domainEvent.NewLevel);

        return Task.CompletedTask;
    }
}
