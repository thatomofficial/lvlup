using LvlUp.Domain.Hunters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LvlUp.Infrastructure.Database.Configurations;

internal sealed class HunterConfiguration : IEntityTypeConfiguration<Hunter>
{
    public void Configure(EntityTypeBuilder<Hunter> builder)
    {
        builder.HasKey(hunter => hunter.Id);

        builder.Property(hunter => hunter.Email).HasMaxLength(256);
        builder.HasIndex(hunter => hunter.Email).IsUnique();

        builder.Property(hunter => hunter.Name).HasMaxLength(50);
        builder.Property(hunter => hunter.Surname).HasMaxLength(50);

        builder.Property(hunter => hunter.Username).HasMaxLength(30);
        builder.HasIndex(hunter => hunter.Username).IsUnique();

        builder.Property(hunter => hunter.PasswordHash).HasMaxLength(512);
        builder.Property(hunter => hunter.GitHubUsername).HasMaxLength(39);
        builder.Property(hunter => hunter.AvatarPath).HasMaxLength(260);
        builder.Property(hunter => hunter.PasswordResetCodeHash).HasMaxLength(512);

        // Stats live in their own table, one row per hunter, keyed and
        // linked back by hunter_id.
        builder.OwnsOne(hunter => hunter.Stats, stats =>
        {
            stats.ToTable("hunter_stats");
            stats.WithOwner().HasForeignKey("HunterId");
            stats.HasKey("HunterId");
            stats.Property<Guid>("HunterId").HasColumnName("hunter_id");
        });

        builder.Navigation(hunter => hunter.Stats).IsRequired();

        builder.Ignore(hunter => hunter.DomainEvents);
    }
}
