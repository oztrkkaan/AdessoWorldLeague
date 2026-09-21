using AdessoWorldLeague.Domain.Entities;

namespace AdessoWorldLeague.Domain.Tests.Entities;

public class TeamTests
{
    [Fact]
    public void Constructor_ShouldSetNameAndCountry()
    {
        var country = new Country(1, "Türkiye");

        var team = new Team("Adesso İstanbul", country);

        Assert.Equal("Adesso İstanbul", team.Name);
        Assert.Same(country, team.Country);
    }

    [Fact]
    public void Constructor_ShouldNotInferCountryIdFromCountry()
    {
        // CountryId ayrı bir init property olduğundan constructor tarafından doldurulmaz.
        // Kura algoritması CountryId üzerinden çalıştığı için bu davranış bilinçli olarak sabitlenir.
        var team = new Team("Adesso İstanbul", new Country(1, "Türkiye"));

        Assert.Equal(0, team.CountryId);
    }

    [Fact]
    public void ObjectInitializer_ShouldSetIdAndCountryId()
    {
        var country = new Country(3, "Belçika");

        var team = new Team("Adesso Brüksel", country) { Id = 9, CountryId = 3 };

        Assert.Equal(9, team.Id);
        Assert.Equal(3, team.CountryId);
    }
}
