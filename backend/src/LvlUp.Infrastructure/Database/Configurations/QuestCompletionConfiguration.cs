using LvlUp.Domain.Quests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LvlUp.Infrastructure.Database.Configurations;

internal sealed class QuestCompletionConfiguration : IEntityTypeConfiguration<QuestCompletion>
{
    public void Configure(EntityTypeBuilder<QuestCompletion> builder)
    {
        builder.HasKey(completion => completion.Id);

        builder.Property(completion => completion.Note).HasMaxLength(280);

        // Intentionally no FK to Quest: completions are an immutable history log
        // that must survive quest deletion.
        builder.HasIndex(completion => completion.HunterId);
    }
}
