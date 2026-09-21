using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Application.Tests.TestData;

public static class TestDbContextFactory
{
    public const int CountryCount = 8;
    public const int TeamsPerCountry = 4;

    /// <summary>
    /// Her çağrıda izole bir in-memory veritabanı oluşturur.
    /// <paramref name="teamsPerCountry"/> = 0 verilirse takım eklenmez.
    /// </summary>
    public static TestApplicationDbContext Create(
        int countryCount = CountryCount,
        int teamsPerCountry = TeamsPerCountry)
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase($"adesso-world-league-{Guid.NewGuid()}")
            .Options;

        var context = new TestApplicationDbContext(options);
        Seed(context, countryCount, teamsPerCountry);

        return context;
    }

    private static void Seed(TestApplicationDbContext context, int countryCount, int teamsPerCountry)
    {
        if (countryCount == 0 || teamsPerCountry == 0)
        {
            return;
        }

        var teamId = 1;

        for (var countryId = 1; countryId <= countryCount; countryId++)
        {
            var country = new Country(countryId, $"Country-{countryId}");
            context.Countries.Add(country);

            for (var i = 1; i <= teamsPerCountry; i++)
            {
                context.Teams.Add(new Team($"Team-{countryId}-{i}", country)
                {
                    Id = teamId++,
                    CountryId = countryId
                });
            }
        }

        context.SaveChanges();
        context.ChangeTracker.Clear();
    }
}
