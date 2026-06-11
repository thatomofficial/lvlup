using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Events;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Hunter> Hunters => Set<Hunter>();

    public DbSet<Quest> Quests => Set<Quest>();

    public DbSet<QuestCompletion> QuestCompletions => Set<QuestCompletion>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Domain events are dispatched after the changes are persisted (eventual consistency).
        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync(cancellationToken);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        List<IDomainEvent> domainEvents = [.. ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                IDomainEvent[] events = [.. entity.DomainEvents];
                entity.ClearDomainEvents();
                return events;
            })];

        if (domainEvents.Count > 0)
        {
            await domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }
    }
}
