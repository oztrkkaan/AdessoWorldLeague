namespace AdessoWorldLeague.Domain.Entities;

public class DrawTeamAssignment
{
    public DrawTeamAssignment(DrawGroup drawGroup, int teamId)
    {
        DrawGroup = drawGroup;
        TeamId = teamId;
    }

    private DrawTeamAssignment() { }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DrawGroupId { get; init; }
    public int TeamId { get; init; }

    public Team Team { get; set; } = null!;
    public DrawGroup DrawGroup { get; set; } = null!;
}
