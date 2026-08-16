using AdessoWorldLeague.Domain.Entities.Contants;
namespace AdessoWorldLeague.Domain.Entities;

public class Draw
{
    public readonly int[] AcceptableGroupCounts = [4, 8];
    public string[] groupNames = ["A", "B", "C", "D", "E", "F", "G", "H"];

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

        List<DrawGroup> drawGroups = groupNames
            .Take(groupCount)
            .Select(groupName => new DrawGroup(groupName, this.Id))
            .ToList();

        var assignedTeamIds = new HashSet<int>();
        int teamsPerGroup = teams.Count / groupCount;

        for (int round = 0; round < teamsPerGroup; round++)
        {
            for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
            {
                var drawGroup = drawGroups[groupIndex];

                var existingCountryIds = drawGroup.DrawTeamAssignments
                    .Select(a => teams.First(t => t.Id == a.TeamId).CountryId)
                    .ToHashSet();

                var randomizedTeam = teams
                    .Where(t => !assignedTeamIds.Contains(t.Id) && !existingCountryIds.Contains(t.CountryId))
                    .OrderBy(_ => Guid.NewGuid())
                    .FirstOrDefault();

                if (randomizedTeam is null)
                    throw new InvalidOperationException($"No eligible team found for group {drawGroup.GroupName} in round {round + 1}.");

                drawGroup.DrawTeamAssignments.Add(new(drawGroup: drawGroup, teamId: randomizedTeam.Id));
                assignedTeamIds.Add(randomizedTeam.Id);
            }
        }

        DrawGroups = drawGroups;
    }

    public void SetCreatorFullName(string creatorFullName)
    {
        if (string.IsNullOrEmpty(creatorFullName))
        {
            throw new ArgumentNullException("Creator Fullname cannot be null or empty");
        }

        CreatorFullName = creatorFullName;
    }
    public void ThrowIfInvalidGroupCount(int groupCount)
    {
        if (!AcceptableGroupCounts.Contains(groupCount))
        {
            throw new ArgumentException($"Group count must be one of the following values: {string.Join(", ", AcceptableGroupCounts)}");
        }
    }

    public void ThrowIfInvalidTeamCount(int teamCount)
    {
        if (teamCount != DrawConstants.MaxTeamsCount)
        {
            throw new ArgumentException($"Team count must be {DrawConstants.MaxTeamsCount}");
        }
    }
}
