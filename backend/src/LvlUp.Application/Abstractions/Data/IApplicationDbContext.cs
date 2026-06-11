using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Hunter> Hunters { get; }

    DbSet<Quest> Quests { get; }

    DbSet<QuestCompletion> QuestCompletions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
