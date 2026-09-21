using AdessoWorldLeague.Domain.Entities;

namespace AdessoWorldLeague.Domain.Tests.Entities;

public class CountryTests
{
    [Fact]
    public void Constructor_ShouldSetIdAndName()
    {
        var country = new Country(1, "Türkiye");

        Assert.Equal(1, country.Id);
        Assert.Equal("Türkiye", country.Name);
    }

    [Fact]
    public void Constructor_ShouldStartWithEmptyTeamCollection()
    {
        var country = new Country(1, "Türkiye");

        Assert.NotNull(country.Teams);
        Assert.Empty(country.Teams);
    }

    [Fact]
    public void Teams_ShouldAcceptRelatedTeams()
    {
        var country = new Country(1, "Türkiye");

        country.Teams!.Add(new Team("Adesso İstanbul", country) { Id = 1, CountryId = 1 });

        var team = Assert.Single(country.Teams);
        Assert.Equal("Adesso İstanbul", team.Name);
    }
}
