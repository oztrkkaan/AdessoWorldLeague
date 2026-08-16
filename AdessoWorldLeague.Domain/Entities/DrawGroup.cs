namespace AdessoWorldLeague.Domain.Entities;

public class DrawGroup
{
    public DrawGroup(string groupName, Guid drawId)
    {
        GroupName = groupName;
        DrawId = drawId;
    }

    private DrawGroup() { }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string GroupName { get; init; } = null!;
    public Guid DrawId { get; set; }

    public Draw Draw { get; private set; } = null!;

    public List<Team> Teams { get; private set; } = [];
    public List<DrawTeamAssignment> DrawTeamAssignments { get; private set; } = [];
}
