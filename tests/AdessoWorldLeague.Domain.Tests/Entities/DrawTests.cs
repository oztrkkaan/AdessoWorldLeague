using AdessoWorldLeague.Domain.Entities;
using AdessoWorldLeague.Domain.Entities.Contants;
using AdessoWorldLeague.Domain.Tests.TestData;

namespace AdessoWorldLeague.Domain.Tests.Entities;

public class DrawTests
{
    private const string Creator = "Kaan Öztürk";

    [Fact]
    public void Constructor_ShouldSetCreatorFullName()
    {
        var draw = new Draw(Creator);

        Assert.Equal(Creator, draw.CreatorFullName);
    }

    [Fact]
    public void Constructor_ShouldGenerateNonEmptyId()
    {
        var draw = new Draw(Creator);

        Assert.NotEqual(Guid.Empty, draw.Id);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIdPerInstance()
    {
        var first = new Draw(Creator);
        var second = new Draw(Creator);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Constructor_ShouldStartWithEmptyDrawGroups()
    {
        var draw = new Draw(Creator);

        Assert.Empty(draw.DrawGroups);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_ShouldThrow_WhenCreatorFullNameIsNullOrEmpty(string? creatorFullName)
    {
        Assert.Throws<ArgumentNullException>(() => new Draw(creatorFullName!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(9)]
    [InlineData(16)]
    [InlineData(-4)]
    public void Make_ShouldThrowArgumentException_WhenGroupCountIsNotAcceptable(int groupCount)
    {
        var draw = new Draw(Creator);
        var teams = TeamFactory.CreateTeams();

        var exception = Assert.Throws<ArgumentException>(() => draw.Make(teams, groupCount));
        Assert.Contains("Group count must be one of the following values", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(33)]
    [InlineData(64)]
    public void Make_ShouldThrowArgumentException_WhenTeamCountIsNotMaxTeamsCount(int teamCount)
    {
        var draw = new Draw(Creator);
        var teams = TeamFactory.CreateTeams().Take(teamCount).ToList();
        while (teams.Count < teamCount)
        {
            teams.Add(new Team($"Extra-{teams.Count}", new Country(99, "Extra"))
            {
                Id = 1000 + teams.Count,
                CountryId = 99
            });
        }

        var exception = Assert.Throws<ArgumentException>(
            () => draw.Make(teams, DrawConstants.AcceptableGroupCounts[0]));

        Assert.Contains($"Team count must be {DrawConstants.MaxTeamsCount}", exception.Message);
    }

    [Fact]
    public void Make_ShouldValidateGroupCountBeforeTeamCount()
    {
        var draw = new Draw(Creator);

        var exception = Assert.Throws<ArgumentException>(() => draw.Make([], groupCount: 3));

        Assert.Contains("Group count", exception.Message);
    }

    [Fact]
    public void Make_ShouldNotMutateDrawGroups_WhenValidationFails()
    {
        var draw = new Draw(Creator);

        Assert.Throws<ArgumentException>(() => draw.Make(TeamFactory.CreateTeams(), groupCount: 3));

        Assert.Empty(draw.DrawGroups);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Make_ShouldCreateExpectedNumberOfGroups(int groupCount)
    {
        var draw = MakeDraw(TeamFactory.CreateTeams(), groupCount);

        Assert.Equal(groupCount, draw.DrawGroups.Count);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Make_ShouldUseGroupNamesInOrder(int groupCount)
    {
        var draw = MakeDraw(TeamFactory.CreateTeams(), groupCount);

        var expected = DrawConstants.GroupNames.Take(groupCount).ToArray();
        Assert.Equal(expected, draw.DrawGroups.Select(g => g.GroupName).ToArray());
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Make_ShouldDistributeTeamsEvenlyAcrossGroups(int groupCount)
    {
        var draw = MakeDraw(TeamFactory.CreateTeams(), groupCount);

        var expectedPerGroup = DrawConstants.MaxTeamsCount / groupCount;
        Assert.All(draw.DrawGroups, group => Assert.Equal(expectedPerGroup, group.DrawTeamAssignments.Count));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Make_ShouldAssignEveryTeamExactlyOnce(int groupCount)
    {
        var teams = TeamFactory.CreateTeams();

        var draw = MakeDraw(teams, groupCount);

        var assignedTeamIds = draw.DrawGroups
            .SelectMany(g => g.DrawTeamAssignments)
            .Select(a => a.TeamId)
            .ToList();

        Assert.Equal(DrawConstants.MaxTeamsCount, assignedTeamIds.Count);
        Assert.Equal(DrawConstants.MaxTeamsCount, assignedTeamIds.Distinct().Count());
        Assert.Equal(teams.Select(t => t.Id).OrderBy(id => id), assignedTeamIds.OrderBy(id => id));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Make_ShouldNotPlaceTwoTeamsOfSameCountryInSameGroup(int groupCount)
    {
        var teams = TeamFactory.CreateTeams();

        var draw = MakeDraw(teams, groupCount);

        foreach (var group in draw.DrawGroups)
        {
            var countryIds = group.DrawTeamAssignments
                .Select(a => teams.First(t => t.Id == a.TeamId).CountryId)
                .ToList();

            Assert.Equal(countryIds.Count, countryIds.Distinct().Count());
        }
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Make_ShouldLinkEveryGroupToTheOwningDraw(int groupCount)
    {
        var draw = MakeDraw(TeamFactory.CreateTeams(), groupCount);

        Assert.All(draw.DrawGroups, group => Assert.Equal(draw.Id, group.DrawId));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Make_ShouldLinkEveryAssignmentToItsGroup(int groupCount)
    {
        var draw = MakeDraw(TeamFactory.CreateTeams(), groupCount);

        foreach (var group in draw.DrawGroups)
        {
            Assert.All(group.DrawTeamAssignments, assignment => Assert.Same(group, assignment.DrawGroup));
        }
    }

    [Fact]
    public void Make_ShouldReplacePreviousResult_WhenCalledTwice()
    {
        var draw = new Draw(Creator);
        var teams = TeamFactory.CreateTeams();

        draw.Make(teams, groupCount: 4);
        Assert.Equal(4, draw.DrawGroups.Count);

        draw.Make(teams, groupCount: 8);

        Assert.Equal(8, draw.DrawGroups.Count);
        Assert.Equal(DrawConstants.MaxTeamsCount, draw.DrawGroups.Sum(g => g.DrawTeamAssignments.Count));
    }

    [Fact]
    public void Make_ShouldProduceDifferentDistributions_AcrossMultipleRuns()
    {
        var teams = TeamFactory.CreateTeams();
        var signatures = new HashSet<string>();

        for (var i = 0; i < 25; i++)
        {
            var draw = new Draw(Creator);
            draw.Make(teams, groupCount: 4);

            signatures.Add(string.Join("|", draw.DrawGroups
                .Select(g => $"{g.GroupName}:{string.Join(",", g.DrawTeamAssignments.Select(a => a.TeamId).OrderBy(id => id))}")));
        }

        Assert.True(signatures.Count > 1, "Kura çekimi rastgele olmalı, her seferinde aynı dağılımı üretmemeli.");
    }

    [Fact]
    public void Make_ShouldThrowInvalidOperationException_WhenOneCountryHasMoreTeamsThanGroups()
    {
        var draw = new Draw(Creator);
        var teams = TeamFactory.CreateSingleCountryTeams();

        var exception = Assert.Throws<InvalidOperationException>(() => draw.Make(teams, groupCount: 4));

        Assert.Contains("each group can contain at most one team from the same country", exception.Message);
    }

    [Fact]
    public void Make_ShouldThrowInvalidOperationException_WhenCountryHasMoreTeamsThanGroups()
    {
        // 4 ülke x 8 takım = 32 takım. 4 grup varken bir ülkenin 8 takımı olduğundan
        // en az iki takımın aynı gruba düşmesi kaçınılmazdır.
        var draw = new Draw(Creator);
        var teams = TeamFactory.CreateTeams(countryCount: 4, teamsPerCountry: 8);

        var exception = Assert.Throws<InvalidOperationException>(() => draw.Make(teams, groupCount: 4));

        Assert.Contains("A country has 8 teams but there are only 4 groups", exception.Message);
    }

    [Fact]
    public void Make_ShouldSucceed_WhenEightGroupsHaveExactlyEnoughCountries()
    {
        // 8 grup için her gruba 4 farklı ülkeden takım gerekir; 8 ülkenin 4'er takımı yeterlidir.
        var draw = new Draw(Creator);
        var teams = TeamFactory.CreateTeams();

        draw.Make(teams, groupCount: 8);

        Assert.All(draw.DrawGroups, group => Assert.Equal(4, group.DrawTeamAssignments.Count));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Make_ShouldNeverFail_ForValidLeagueSetup(int groupCount)
    {
        // Regresyon: önceki greedy algoritma geri izleme yapmadığı için groupCount = 8'de
        // çağrıların yaklaşık %29'u "No eligible team found" hatasıyla sonuçlanıyordu.
        var teams = TeamFactory.CreateTeams();

        for (var i = 0; i < 500; i++)
        {
            var draw = new Draw(Creator);
            var exception = Record.Exception(() => draw.Make(teams, groupCount));

            Assert.Null(exception);
        }
    }

    private static Draw MakeDraw(List<Team> teams, int groupCount)
    {
        var draw = new Draw(Creator);
        draw.Make(teams, groupCount);

        return draw;
    }
}
