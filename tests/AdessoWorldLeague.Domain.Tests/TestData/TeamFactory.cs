using AdessoWorldLeague.Domain.Entities;
using AdessoWorldLeague.Domain.Entities.Contants;

namespace AdessoWorldLeague.Domain.Tests.TestData;

/// <summary>
/// Gerçek seed verisiyle aynı şekle sahip takım listeleri üretir:
/// 8 ülke x 4 takım = 32 takım.
/// </summary>
public static class TeamFactory
{
    public const int DefaultCountryCount = 8;
    public const int DefaultTeamsPerCountry = 4;

    public static List<Team> CreateTeams(
        int countryCount = DefaultCountryCount,
        int teamsPerCountry = DefaultTeamsPerCountry)
    {
        var teams = new List<Team>();
        var teamId = 1;

        for (var countryId = 1; countryId <= countryCount; countryId++)
        {
            var country = new Country(countryId, $"Country-{countryId}");

            for (var i = 1; i <= teamsPerCountry; i++)
            {
                teams.Add(new Team($"Team-{countryId}-{i}", country)
                {
                    Id = teamId++,
                    CountryId = countryId
                });
            }
        }

        return teams;
    }

    /// <summary>
    /// Takım sayısı <see cref="DrawConstants.MaxTeamsCount"/> olan ancak
    /// tüm takımları tek ülkeye ait olan liste. Ülke kısıtının ihlal
    /// edilemediği senaryoları test etmek için kullanılır.
    /// </summary>
    public static List<Team> CreateSingleCountryTeams()
        => CreateTeams(countryCount: 1, teamsPerCountry: DrawConstants.MaxTeamsCount);
}
