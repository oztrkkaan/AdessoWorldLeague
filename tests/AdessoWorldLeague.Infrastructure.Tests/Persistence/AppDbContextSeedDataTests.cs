using AdessoWorldLeague.Domain.Entities;
using AdessoWorldLeague.Domain.Entities.Contants;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Infrastructure.Tests.Persistence;

public class AppDbContextSeedDataTests : IClassFixture<SqliteAppDbContextFixture>
{
    private readonly SqliteAppDbContextFixture _fixture;

    public AppDbContextSeedDataTests(SqliteAppDbContextFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Seed_ShouldContainEightCountries()
    {
        using var context = _fixture.CreateContext();

        Assert.Equal(8, await context.Countries.CountAsync());
    }

    [Fact]
    public async Task Seed_ShouldContainExpectedCountryNames()
    {
        using var context = _fixture.CreateContext();

        var names = await context.Countries.OrderBy(c => c.Id).Select(c => c.Name).ToListAsync();

        Assert.Equal(
            ["Türkiye", "Almanya", "Belçika", "Fransa", "Hollanda", "Portekiz", "İtalya", "İspanya"],
            names);
    }

    [Fact]
    public async Task Seed_ShouldContainMaxTeamsCountTeams()
    {
        using var context = _fixture.CreateContext();

        Assert.Equal(DrawConstants.MaxTeamsCount, await context.Teams.CountAsync());
    }

    [Fact]
    public async Task Seed_ShouldAssignFourTeamsToEveryCountry()
    {
        using var context = _fixture.CreateContext();

        var teamsPerCountry = await context.Teams
            .GroupBy(t => t.CountryId)
            .Select(g => new { CountryId = g.Key, Count = g.Count() })
            .ToListAsync();

        Assert.Equal(8, teamsPerCountry.Count);
        Assert.All(teamsPerCountry, entry => Assert.Equal(4, entry.Count));
    }

    [Fact]
    public async Task Seed_ShouldGiveEveryTeamAnExistingCountry()
    {
        using var context = _fixture.CreateContext();

        var countryIds = await context.Countries.Select(c => c.Id).ToListAsync();
        var teamCountryIds = await context.Teams.Select(t => t.CountryId).Distinct().ToListAsync();

        Assert.All(teamCountryIds, countryId => Assert.Contains(countryId, countryIds));
    }

    [Theory]
    [InlineData("Türkiye", "Adesso İstanbul", "Adesso Ankara", "Adesso İzmir", "Adesso Antalya")]
    [InlineData("Almanya", "Adesso Berlin", "Adesso Frankfurt", "Adesso Münih", "Adesso Dortmund")]
    [InlineData("Fransa", "Adesso Paris", "Adesso Marsilya", "Adesso Nice", "Adesso Lyon")]
    [InlineData("Hollanda", "Adesso Amsterdam", "Adesso Rotterdam", "Adesso Lahey", "Adesso Eindhoven")]
    [InlineData("Portekiz", "Adesso Lisbon", "Adesso Porto", "Adesso Braga", "Adesso Coimbra")]
    [InlineData("İtalya", "Adesso Roma", "Adesso Milano", "Adesso Venedik", "Adesso Napoli")]
    [InlineData("İspanya", "Adesso Sevilla", "Adesso Madrid", "Adesso Barselona", "Adesso Granada")]
    [InlineData("Belçika", "Adesso Brüksel", "Adesso Brugge", "Adesso Gent", "Adesso Anvers")]
    public async Task Seed_ShouldMatchSpecifiedTeamRoster(string countryName, params string[] expectedTeamNames)
    {
        using var context = _fixture.CreateContext();

        var actualTeamNames = await context.Teams
            .Where(t => t.Country.Name == countryName)
            .Select(t => t.Name)
            .ToListAsync();

        Assert.Equal(expectedTeamNames.OrderBy(n => n), actualTeamNames.OrderBy(n => n));
    }

    [Fact]
    public async Task Seed_ShouldUseUniqueTeamNames()
    {
        using var context = _fixture.CreateContext();

        var names = await context.Teams.Select(t => t.Name).ToListAsync();

        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public async Task Seed_ShouldUseSequentialTeamIds()
    {
        using var context = _fixture.CreateContext();

        var ids = await context.Teams.Select(t => t.Id).OrderBy(id => id).ToListAsync();

        Assert.Equal(Enumerable.Range(1, DrawConstants.MaxTeamsCount), ids);
    }

    [Fact]
    public async Task Seed_ShouldSatisfyDrawPreconditions()
    {
        // Kuranın çalışabilmesi için: tam olarak 32 takım ve her grup sayısı için
        // yeterli sayıda farklı ülke bulunmalıdır.
        using var context = _fixture.CreateContext();

        var teams = await context.Teams.ToListAsync();
        var distinctCountryCount = teams.Select(t => t.CountryId).Distinct().Count();
        var largestGroupCount = DrawConstants.AcceptableGroupCounts.Max();

        Assert.Equal(DrawConstants.MaxTeamsCount, teams.Count);
        Assert.True(distinctCountryCount >= DrawConstants.MaxTeamsCount / largestGroupCount,
            "Her grupta farklı ülkelerden takım olabilmesi için yeterli ülke bulunmalı.");
    }

    [Fact]
    public async Task Teams_ShouldBeLoadableWithCountryNavigation()
    {
        using var context = _fixture.CreateContext();

        var team = await context.Teams.Include(t => t.Country).FirstAsync(t => t.Id == 1);

        Assert.NotNull(team.Country);
        Assert.Equal(team.CountryId, team.Country.Id);
    }

    [Fact]
    public async Task Countries_ShouldBeLoadableWithTeamsNavigation()
    {
        using var context = _fixture.CreateContext();

        var country = await context.Countries.Include(c => c.Teams).FirstAsync(c => c.Id == 1);

        Assert.NotNull(country.Teams);
        Assert.Equal(4, country.Teams.Count);
        Assert.All(country.Teams, team => Assert.Equal(country.Id, team.CountryId));
    }

    [Fact]
    public void DbContext_ShouldExposeAllEntitySets()
    {
        using var context = _fixture.CreateContext();

        Assert.NotNull(context.Countries);
        Assert.NotNull(context.Teams);
        Assert.NotNull(context.Draws);
        Assert.NotNull(context.DrawGroups);
        Assert.NotNull(context.DrawTeamAssignments);
    }

    [Fact]
    public void DbContext_ShouldImplementApplicationDbContextAbstraction()
    {
        using var context = _fixture.CreateContext();

        var applicationDbContext = Assert.IsAssignableFrom<Application.Abstractions.IApplicationDbContext>(context);

        Assert.Same(context.Teams, applicationDbContext.Teams);
        Assert.Same(context.Draws, applicationDbContext.Draws);
    }

    [Fact]
    public void Model_ShouldRegisterEveryDomainEntity()
    {
        using var context = _fixture.CreateContext();

        var entityTypes = context.Model.GetEntityTypes().Select(e => e.ClrType).ToList();

        Assert.Contains(typeof(Country), entityTypes);
        Assert.Contains(typeof(Team), entityTypes);
        Assert.Contains(typeof(Draw), entityTypes);
        Assert.Contains(typeof(DrawGroup), entityTypes);
        Assert.Contains(typeof(DrawTeamAssignment), entityTypes);
    }
}
