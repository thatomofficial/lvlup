using LvlUp.SharedKernel;

namespace LvlUp.Application.Abstractions.Events;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
