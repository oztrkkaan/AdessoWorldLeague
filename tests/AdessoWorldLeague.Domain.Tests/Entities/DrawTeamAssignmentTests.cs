using AdessoWorldLeague.Domain.Entities;

namespace AdessoWorldLeague.Domain.Tests.Entities;

public class DrawTeamAssignmentTests
{
    [Fact]
    public void Constructor_ShouldSetDrawGroupAndTeamId()
    {
        var group = new DrawGroup("A", Guid.CreateVersion7());

        var assignment = new DrawTeamAssignment(group, teamId: 12);

        Assert.Same(group, assignment.DrawGroup);
        Assert.Equal(12, assignment.TeamId);
    }

    [Fact]
    public void Constructor_ShouldGenerateNonEmptyId()
    {
        var assignment = new DrawTeamAssignment(new DrawGroup("A", Guid.CreateVersion7()), teamId: 1);

        Assert.NotEqual(Guid.Empty, assignment.Id);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIdPerInstance()
    {
        var group = new DrawGroup("A", Guid.CreateVersion7());

        var first = new DrawTeamAssignment(group, teamId: 1);
        var second = new DrawTeamAssignment(group, teamId: 2);

        Assert.NotEqual(first.Id, second.Id);
    }
}
