namespace AdessoWorldLeague.Domain.Entities;

public class DrawTeamAssignment
{
    public DrawTeamAssignment(Guid drawId, DrawGroup drawGroup, int teamId)
    {
        DrawId = drawId;
        DrawGroup = drawGroup;
        TeamId = teamId;
    }
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DrawGroupId { get; init; }
    public int TeamId { get; init; }
    public Guid DrawId { get; init; }

    public Draw Draw { get; set; }
    public Team Team { get; set; }
    public DrawGroup DrawGroup { get; set; }
}
