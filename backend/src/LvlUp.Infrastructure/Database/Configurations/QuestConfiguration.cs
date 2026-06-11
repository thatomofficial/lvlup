using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LvlUp.Infrastructure.Database.Configurations;

internal sealed class QuestConfiguration : IEntityTypeConfiguration<Quest>
{
    public void Configure(EntityTypeBuilder<Quest> builder)
    {
        builder.HasKey(quest => quest.Id);

        builder.Property(quest => quest.Title).HasMaxLength(100);
        builder.Property(quest => quest.Description).HasMaxLength(500);

        builder.HasOne<Hunter>()
            .WithMany()
            .HasForeignKey(quest => quest.HunterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(quest => quest.HunterId);

        builder.Ignore(quest => quest.DomainEvents);
    }
}
