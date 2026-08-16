using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

public class DrawConfiguration : IEntityTypeConfiguration<Draw>
{
    public void Configure(EntityTypeBuilder<Draw> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatorFullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(x => x.DrawGroups)
            .WithOne(x => x.Draw)
            .HasForeignKey(x => x.DrawId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
