using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Draw> Draws => Set<Draw>();
    public DbSet<DrawGroup> DrawGroups => Set<DrawGroup>();
    public DbSet<DrawTeamAssignment> DrawTeamAssignments => Set<DrawTeamAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
