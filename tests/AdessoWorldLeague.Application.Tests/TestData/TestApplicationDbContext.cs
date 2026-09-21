using AdessoWorldLeague.Application.Abstractions;
using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Application.Tests.TestData;

/// <summary>
/// Application katmanı testleri için in-memory <see cref="IApplicationDbContext"/> uygulaması.
/// <para>
/// Infrastructure katmanındaki <c>AppDbContext</c> yerine kasıtlı olarak ayrı bir context
/// kullanılır: Application testleri Infrastructure'a bağımlı olmamalıdır (bkz. mimari testleri).
/// </para>
/// </summary>
public class TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public int SaveChangesCallCount { get; private set; }

    public CancellationToken LastSaveChangesCancellationToken { get; private set; }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Draw> Draws => Set<Draw>();
    public DbSet<DrawGroup> DrawGroups => Set<DrawGroup>();
    public DbSet<DrawTeamAssignment> DrawTeamAssignments => Set<DrawTeamAssignment>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        LastSaveChangesCancellationToken = cancellationToken;
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>().HasKey(x => x.Id);
        modelBuilder.Entity<Country>()
            .HasMany(x => x.Teams)
            .WithOne(x => x.Country)
            .HasForeignKey(x => x.CountryId);

        modelBuilder.Entity<Team>().HasKey(x => x.Id);
        modelBuilder.Entity<Team>().Property(x => x.Id).ValueGeneratedNever();

        modelBuilder.Entity<Draw>().HasKey(x => x.Id);
        modelBuilder.Entity<Draw>()
            .HasMany(x => x.DrawGroups)
            .WithOne(x => x.Draw)
            .HasForeignKey(x => x.DrawId);

        modelBuilder.Entity<DrawGroup>().HasKey(x => x.Id);
        modelBuilder.Entity<DrawGroup>()
            .HasMany(x => x.DrawTeamAssignments)
            .WithOne(x => x.DrawGroup)
            .HasForeignKey(x => x.DrawGroupId);

        modelBuilder.Entity<DrawTeamAssignment>().HasKey(x => x.Id);
        modelBuilder.Entity<DrawTeamAssignment>()
            .HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId);

        base.OnModelCreating(modelBuilder);
    }
}
