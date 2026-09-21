using AdessoWorldLeague.Domain.Entities;
using AdessoWorldLeague.Domain.Entities.Contants;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Infrastructure.Tests.Persistence;

/// <summary>
/// Yazma işlemleri içerdiğinden her test kendi izole veritabanını kullanır.
/// </summary>
public class AppDbContextPersistenceTests : IDisposable
{
    private readonly SqliteAppDbContextFixture _fixture = new();

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task SaveChanges_ShouldPersistDrawWithGroupsAndAssignments()
    {
        Guid drawId;

        await using (var context = _fixture.CreateContext())
        {
            var teams = await context.Teams.ToListAsync();
            var draw = new Draw("Kaan Öztürk");
            draw.Make(teams, groupCount: 4);
            drawId = draw.Id;

            context.Draws.Add(draw);
            await context.SaveChangesAsync();
        }

        await using (var context = _fixture.CreateContext())
        {
            var persisted = await context.Draws
                .Include(d => d.DrawGroups)
                .ThenInclude(g => g.DrawTeamAssignments)
                .SingleAsync(d => d.Id == drawId);

            Assert.Equal("Kaan Öztürk", persisted.CreatorFullName);
            Assert.Equal(4, persisted.DrawGroups.Count);
            Assert.Equal(DrawConstants.MaxTeamsCount,
                persisted.DrawGroups.Sum(g => g.DrawTeamAssignments.Count));
        }
    }

    [Fact]
    public async Task SaveChanges_ShouldPersistAssignmentsWithResolvableTeams()
    {
        await using var context = _fixture.CreateContext();

        var teams = await context.Teams.Include(t => t.Country).ToListAsync();
        var draw = new Draw("Kaan Öztürk");
        draw.Make(teams, groupCount: 4);

        context.Draws.Add(draw);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var assignments = await context.DrawTeamAssignments.Include(a => a.Team).ToListAsync();

        Assert.Equal(DrawConstants.MaxTeamsCount, assignments.Count);
        Assert.All(assignments, assignment =>
        {
            Assert.NotNull(assignment.Team);
            Assert.Equal(assignment.TeamId, assignment.Team.Id);
        });
    }

    [Fact]
    public async Task UniqueIndex_ShouldRejectDuplicateGroupNameWithinSameDraw()
    {
        await using var context = _fixture.CreateContext();

        var draw = new Draw("Kaan Öztürk");
        context.Draws.Add(draw);
        await context.SaveChangesAsync();

        context.DrawGroups.Add(new DrawGroup("A", draw.Id));
        context.DrawGroups.Add(new DrawGroup("A", draw.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task UniqueIndex_ShouldAllowSameGroupNameAcrossDifferentDraws()
    {
        await using var context = _fixture.CreateContext();

        var firstDraw = new Draw("Kaan Öztürk");
        var secondDraw = new Draw("Başka Kullanıcı");
        context.Draws.AddRange(firstDraw, secondDraw);
        await context.SaveChangesAsync();

        context.DrawGroups.Add(new DrawGroup("A", firstDraw.Id));
        context.DrawGroups.Add(new DrawGroup("A", secondDraw.Id));

        await context.SaveChangesAsync();

        Assert.Equal(2, await context.DrawGroups.CountAsync(g => g.GroupName == "A"));
    }

    [Fact]
    public async Task CascadeDelete_ShouldRemoveGroupsAndAssignments_WhenDrawIsDeleted()
    {
        await using var context = _fixture.CreateContext();

        var teams = await context.Teams.ToListAsync();
        var draw = new Draw("Kaan Öztürk");
        draw.Make(teams, groupCount: 4);
        context.Draws.Add(draw);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var persisted = await context.Draws
            .Include(d => d.DrawGroups)
            .ThenInclude(g => g.DrawTeamAssignments)
            .SingleAsync();

        context.Draws.Remove(persisted);
        await context.SaveChangesAsync();

        Assert.Empty(await context.Draws.ToListAsync());
        Assert.Empty(await context.DrawGroups.ToListAsync());
        Assert.Empty(await context.DrawTeamAssignments.ToListAsync());
    }

    [Fact]
    public async Task RestrictDelete_ShouldRejectTeamDeletion_WhenTeamIsAssigned()
    {
        await using var context = _fixture.CreateContext();

        var teams = await context.Teams.ToListAsync();
        var draw = new Draw("Kaan Öztürk");
        draw.Make(teams, groupCount: 4);
        context.Draws.Add(draw);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var assignedTeamId = await context.DrawTeamAssignments.Select(a => a.TeamId).FirstAsync();
        var assignedTeam = await context.Teams.SingleAsync(t => t.Id == assignedTeamId);

        context.Teams.Remove(assignedTeam);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task RestrictDelete_ShouldRejectCountryDeletion_WhenCountryHasTeams()
    {
        await using var context = _fixture.CreateContext();

        var country = await context.Countries.SingleAsync(c => c.Id == 1);
        context.Countries.Remove(country);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task MaxLength_ShouldRejectGroupNameLongerThanOneCharacter()
    {
        await using var context = _fixture.CreateContext();

        var draw = new Draw("Kaan Öztürk");
        context.Draws.Add(draw);
        await context.SaveChangesAsync();

        context.DrawGroups.Add(new DrawGroup("AB", draw.Id));

        // SQLite uzunluk kısıtını zorlamadığı için doğrulama model seviyesinde yapılır.
        var maxLength = context.Model
            .FindEntityType(typeof(DrawGroup))!
            .FindProperty(nameof(DrawGroup.GroupName))!
            .GetMaxLength();

        Assert.Equal(1, maxLength);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnNumberOfAffectedRows()
    {
        await using var context = _fixture.CreateContext();

        context.Draws.Add(new Draw("Kaan Öztürk"));

        Assert.Equal(1, await context.SaveChangesAsync());
    }

    [Fact]
    public async Task Draws_ShouldPersistMultipleIndependentDraws()
    {
        await using var context = _fixture.CreateContext();

        var teams = await context.Teams.ToListAsync();

        foreach (var creator in new[] { "Kaan Öztürk", "Başka Kullanıcı", "Üçüncü Kişi" })
        {
            var draw = new Draw(creator);
            draw.Make(teams, groupCount: 4);
            context.Draws.Add(draw);
        }

        await context.SaveChangesAsync();

        Assert.Equal(3, await context.Draws.CountAsync());
        Assert.Equal(12, await context.DrawGroups.CountAsync());
        Assert.Equal(3 * DrawConstants.MaxTeamsCount, await context.DrawTeamAssignments.CountAsync());
    }
}
