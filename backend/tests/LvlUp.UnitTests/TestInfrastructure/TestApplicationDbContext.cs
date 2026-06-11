using LvlUp.Application.Abstractions.Data;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.UnitTests.TestInfrastructure;

internal sealed class TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Hunter> Hunters => Set<Hunter>();

    public DbSet<Quest> Quests => Set<Quest>();

    public DbSet<QuestCompletion> QuestCompletions => Set<QuestCompletion>();

    public static TestApplicationDbContext Create() =>
        new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hunter>().Ignore(hunter => hunter.DomainEvents);
        modelBuilder.Entity<Quest>().Ignore(quest => quest.DomainEvents);
        modelBuilder.Entity<QuestCompletion>();
    }
}
