using AdessoWorldLeague.Domain.Entities;

namespace AdessoWorldLeague.Domain.Tests.Entities;

public class DrawGroupTests
{
    [Fact]
    public void Constructor_ShouldSetGroupNameAndDrawId()
    {
        var drawId = Guid.CreateVersion7();

        var group = new DrawGroup("A", drawId);

        Assert.Equal("A", group.GroupName);
        Assert.Equal(drawId, group.DrawId);
    }

    [Fact]
    public void Constructor_ShouldGenerateNonEmptyId()
    {
        var group = new DrawGroup("A", Guid.CreateVersion7());

        Assert.NotEqual(Guid.Empty, group.Id);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIdPerInstance()
    {
        var drawId = Guid.CreateVersion7();

        var first = new DrawGroup("A", drawId);
        var second = new DrawGroup("B", drawId);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Constructor_ShouldStartWithEmptyAssignments()
    {
        var group = new DrawGroup("A", Guid.CreateVersion7());

        Assert.Empty(group.DrawTeamAssignments);
    }

    [Fact]
    public void DrawTeamAssignments_ShouldAcceptNewAssignments()
    {
        var group = new DrawGroup("A", Guid.CreateVersion7());

        group.DrawTeamAssignments.Add(new DrawTeamAssignment(group, teamId: 7));

        var assignment = Assert.Single(group.DrawTeamAssignments);
        Assert.Equal(7, assignment.TeamId);
        Assert.Same(group, assignment.DrawGroup);
    }
}
