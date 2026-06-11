using LvlUp.Application.Abstractions.Events;
using LvlUp.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace LvlUp.Infrastructure.Database;

internal sealed class DomainEventsDispatcher(IServiceProvider serviceProvider) : IDomainEventsDispatcher
{
    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());

            foreach (object? handler in serviceProvider.GetServices(handlerType))
            {
                if (handler is null)
                {
                    continue;
                }

                HandlerWrapper wrapper = HandlerWrapper.Create(handler, domainEvent.GetType());
                await wrapper.HandleAsync(domainEvent, cancellationToken);
            }
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);

        public static HandlerWrapper Create(object handler, Type domainEventType)
        {
            Type wrapperType = typeof(HandlerWrapper<>).MakeGenericType(domainEventType);
            return (HandlerWrapper)Activator.CreateInstance(wrapperType, handler)!;
        }
    }

    private sealed class HandlerWrapper<TDomainEvent>(object handler) : HandlerWrapper
        where TDomainEvent : IDomainEvent
    {
        public override Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken) =>
            ((IDomainEventHandler<TDomainEvent>)handler).HandleAsync((TDomainEvent)domainEvent, cancellationToken);
    }
}
