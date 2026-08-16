using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

public class DrawTeamAssignmentConfiguration: IEntityTypeConfiguration<DrawTeamAssignment>
{
    public void Configure(EntityTypeBuilder<DrawTeamAssignment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
