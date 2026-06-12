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

        builder.Ignore(hunter => hunter.DomainEvents);
    }
}
