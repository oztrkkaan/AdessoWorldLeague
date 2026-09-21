using AdessoWorldLeague.Domain.Entities.Contants;
namespace AdessoWorldLeague.Domain.Entities;

public class Draw
{

    public Draw(string creatorFullName)
    {
        SetCreatorFullName(creatorFullName);
    }

    private Draw() { }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string CreatorFullName { get; private set; } = null!;
    public ICollection<DrawGroup> DrawGroups { get; private set; } = [];


    public void Make(List<Team> teams, int groupCount)
    {
        ThrowIfInvalidGroupCount(groupCount);
        ThrowIfInvalidTeamCount(teams.Count);
        ThrowIfCountriesCannotFillGroups(teams, groupCount);

        List<DrawGroup> drawGroups = DrawConstants.GroupNames
            .Take(groupCount)
            .Select(groupName => new DrawGroup(groupName, this.Id))
            .ToList();

        // Takımlar rastgele sıraya alınır; kura sırası bu sıralamadan belirlenir.
        var shuffledTeams = teams.ToArray();
        Random.Shared.Shuffle(shuffledTeams);

        var assignedTeams = new bool[shuffledTeams.Length];
        var countryIdsPerGroup = Enumerable.Range(0, groupCount)
            .Select(_ => new HashSet<int>())
            .ToList();

        if (!TryDrawTeamIntoGroup(
                slotIndex: 0,
                totalSlots: teams.Count,
                groupCount: groupCount,
                teams: shuffledTeams,
                assignedTeams: assignedTeams,
                drawGroups: drawGroups,
                countryIdsPerGroup: countryIdsPerGroup))
        {
            throw new InvalidOperationException(
                "No valid draw could be produced for the given teams and group count.");
        }

        DrawGroups = drawGroups;
    }

    private static bool TryDrawTeamIntoGroup(
        int slotIndex,
        int totalSlots,
        int groupCount,
        Team[] teams,
        bool[] assignedTeams,
        List<DrawGroup> drawGroups,
        List<HashSet<int>> countryIdsPerGroup)
    {
        if (slotIndex == totalSlots)
            return true;

        var groupIndex = slotIndex % groupCount;
        var drawGroup = drawGroups[groupIndex];
        var countryIdsInGroup = countryIdsPerGroup[groupIndex];

        for (var i = 0; i < teams.Length; i++)
        {
            if (assignedTeams[i])
                continue;

            var team = teams[i];

            if (countryIdsInGroup.Contains(team.CountryId))
                continue;

            var assignment = new DrawTeamAssignment(drawGroup: drawGroup, teamId: team.Id);

            assignedTeams[i] = true;
            countryIdsInGroup.Add(team.CountryId);
            drawGroup.DrawTeamAssignments.Add(assignment);

            if (TryDrawTeamIntoGroup(
                    slotIndex + 1, totalSlots, groupCount,
                    teams, assignedTeams, drawGroups, countryIdsPerGroup))
            {
                return true;
            }

            drawGroup.DrawTeamAssignments.Remove(assignment);
            countryIdsInGroup.Remove(team.CountryId);
            assignedTeams[i] = false;
        }

        return false;
    }

    private void SetCreatorFullName(string creatorFullName)
    {
        if (string.IsNullOrEmpty(creatorFullName))
        {
            throw new ArgumentNullException("Creator Fullname cannot be null or empty");
        }

        CreatorFullName = creatorFullName;
    }
    private void ThrowIfInvalidGroupCount(int groupCount)
    {
        if (!DrawConstants.AcceptableGroupCounts.Contains(groupCount))
        {
            throw new ArgumentException($"Group count must be one of the following values: {string.Join(", ", DrawConstants.AcceptableGroupCounts)}");
        }
    }

    private void ThrowIfInvalidTeamCount(int teamCount)
    {
        if (teamCount != DrawConstants.MaxTeamsCount)
        {
            throw new ArgumentException($"Team count must be {DrawConstants.MaxTeamsCount}");
        }
    }
    private static void ThrowIfCountriesCannotFillGroups(List<Team> teams, int groupCount)
    {
        var teamCountsByCountry = teams
            .GroupBy(team => team.CountryId)
            .Select(group => group.Count())
            .ToList();

        var largestCountry = teamCountsByCountry.Max();
        if (largestCountry > groupCount)
        {
            throw new InvalidOperationException(
                $"A country has {largestCountry} teams but there are only {groupCount} groups; " +
                "each group can contain at most one team from the same country.");
        }
    }
}
