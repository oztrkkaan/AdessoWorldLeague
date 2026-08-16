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
    public List<DrawTeamAssignment> DrawTeamAssignments { get; private set; } = [];
    public List<DrawGroup> DrawGroups { get; private set; } = [];


    public void Make(List<Country> countries, int groupCount)
    {
        ThrowIfInvalidGroupCount(groupCount);

        int teamsCount = countries.Sum(c => c.Teams.Count);
        ThrowIfInvalidTeamCount(teamsCount);

        List<DrawGroup> groups = groupNames
            .Take(groupCount)
            .Select(groupName => new DrawGroup(groupName, this.Id))
            .ToList();

        List<DrawTeamAssignment> drawTeamAssignments = [];

        for (int i = 0; i < teamsCount; i++)
        {
            int groupIndex = i % groupCount;

            var group = groups[groupIndex];

            var existingCountriesInGroup = group.Teams
                .Select(t => t.CountryId)
                .ToHashSet();
            var existingTeamIdsInGroup = group.Teams
                .Select(t => t.Id)
                .ToHashSet();

            var randomizedTeam = countries
                .Where(c => !existingCountriesInGroup.Contains(c.Id) && !existingTeamIdsInGroup.Contains(c.Id))
                .SelectMany(c => c.Teams)
                .OrderBy(t => Guid.NewGuid())
                .FirstOrDefault()!;

            group.Teams.Add(randomizedTeam);

            drawTeamAssignments.Add(new(drawId: this.Id,
                                        drawGroup: group,
                                        teamId: randomizedTeam.Id));
        }

        DrawTeamAssignments = drawTeamAssignments;
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
