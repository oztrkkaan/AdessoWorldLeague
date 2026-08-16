using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

public class DrawGroupConfiguration : IEntityTypeConfiguration<DrawGroup>
{
    public void Configure(EntityTypeBuilder<DrawGroup> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GroupName)
            .IsRequired()
            .HasMaxLength(1);

        builder.HasIndex(x => new { x.DrawId, x.GroupName })
            .IsUnique();

        builder.HasMany(e => e.DrawTeamAssignments)
               .WithOne(e => e.DrawGroup)
               .HasForeignKey(e => e.DrawGroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
