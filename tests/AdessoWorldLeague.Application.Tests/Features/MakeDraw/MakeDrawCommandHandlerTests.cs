using System.Text.Json;
using AdessoWorldLeague.Application.Features.MakeDraw;
using AdessoWorldLeague.Application.Tests.TestData;
using AdessoWorldLeague.Domain.Entities.Contants;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Application.Tests.Features.MakeDraw;

public class MakeDrawCommandHandlerTests
{
    private const string Creator = "Kaan Öztürk";

    [Fact]
    public async Task Handle_ShouldReturnResponseWithPersistedDrawId()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        var response = await HandleAsync(handler, new MakeDrawCommand(Creator, 4));

        Assert.NotEqual(Guid.Empty, response.DrawId);

        var persistedDraw = await context.Draws.SingleAsync();
        Assert.Equal(persistedDraw.Id, response.DrawId);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public async Task Handle_ShouldReturnRequestedNumberOfGroups(int groupCount)
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        var response = await HandleAsync(handler, new MakeDrawCommand(Creator, groupCount));

        Assert.Equal(groupCount, response.Groups.Count);
        Assert.Equal(DrawConstants.GroupNames.Take(groupCount), response.Groups.Select(g => g.GroupName));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public async Task Handle_ShouldReturnEveryTeamNameExactlyOnce(int groupCount)
    {
        using var context = TestDbContextFactory.Create();
        var expectedTeamNames = await context.Teams.Select(t => t.Name).ToListAsync();
        var handler = new MakeDrawCommandHandler(context);

        var response = await HandleAsync(handler, new MakeDrawCommand(Creator, groupCount));

        var returnedTeamNames = response.Groups.SelectMany(g => g.Teams).Select(t => t.Name).ToList();

        Assert.Equal(DrawConstants.MaxTeamsCount, returnedTeamNames.Count);
        Assert.Equal(expectedTeamNames.OrderBy(n => n), returnedTeamNames.OrderBy(n => n));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public async Task Handle_ShouldDistributeTeamsEvenlyAcrossReturnedGroups(int groupCount)
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        var response = await HandleAsync(handler, new MakeDrawCommand(Creator, groupCount));

        var expectedPerGroup = DrawConstants.MaxTeamsCount / groupCount;
        Assert.All(response.Groups, group => Assert.Equal(expectedPerGroup, group.Teams.Count));
    }

    [Fact]
    public async Task Handle_ShouldNotReturnTwoTeamsOfSameCountryInSameGroup()
    {
        using var context = TestDbContextFactory.Create();
        var teamsByName = await context.Teams.ToDictionaryAsync(t => t.Name, t => t.CountryId);
        var handler = new MakeDrawCommandHandler(context);

        var response = await HandleAsync(handler, new MakeDrawCommand(Creator, 4));

        foreach (var group in response.Groups)
        {
            var countryIds = group.Teams.Select(team => teamsByName[team.Name]).ToList();
            Assert.Equal(countryIds.Count, countryIds.Distinct().Count());
        }
    }

    [Fact]
    public async Task Handle_ShouldPersistDrawWithCreatorFullName()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        await HandleAsync(handler, new MakeDrawCommand(Creator, 4));

        var persistedDraw = await context.Draws.SingleAsync();
        Assert.Equal(Creator, persistedDraw.CreatorFullName);
    }

    [Fact]
    public async Task Handle_ShouldPersistGroupsAndAssignments()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        var response = await HandleAsync(handler, new MakeDrawCommand(Creator, 4));

        var persistedGroups = await context.DrawGroups
            .Where(g => g.DrawId == response.DrawId)
            .ToListAsync();
        var persistedAssignments = await context.DrawTeamAssignments.ToListAsync();

        Assert.Equal(4, persistedGroups.Count);
        Assert.Equal(DrawConstants.MaxTeamsCount, persistedAssignments.Count);
        Assert.All(persistedAssignments,
            assignment => Assert.Contains(persistedGroups, g => g.Id == assignment.DrawGroupId));
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsyncExactlyOnce()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        await handler.Handle(new MakeDrawCommand(Creator, 4), CancellationToken.None);

        Assert.Equal(1, context.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ShouldForwardCancellationTokenToSaveChangesAsync()
    {
        using var context = TestDbContextFactory.Create();
        using var cts = new CancellationTokenSource();
        var handler = new MakeDrawCommandHandler(context);

        await handler.Handle(new MakeDrawCommand(Creator, 4), cts.Token);

        Assert.Equal(cts.Token, context.LastSaveChangesCancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldThrowOperationCanceledException_WhenTokenIsAlreadyCancelled()
    {
        using var context = TestDbContextFactory.Create();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var handler = new MakeDrawCommandHandler(context);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => handler.Handle(new MakeDrawCommand(Creator, 4), cts.Token));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(16)]
    public async Task Handle_ShouldThrowArgumentException_WhenGroupCountIsNotAcceptable(int groupCount)
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new MakeDrawCommand(Creator, groupCount), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenTeamCountIsNotMaxTeamsCount()
    {
        using var context = TestDbContextFactory.Create(countryCount: 4, teamsPerCountry: 4);
        var handler = new MakeDrawCommandHandler(context);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new MakeDrawCommand(Creator, 4), CancellationToken.None));

        Assert.Contains($"Team count must be {DrawConstants.MaxTeamsCount}", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenCreatorFullNameIsEmpty()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.Handle(new MakeDrawCommand(string.Empty, 4), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotPersistAnything_WhenDrawFails()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new MakeDrawCommand(Creator, GroupCount: 3), CancellationToken.None));

        Assert.Equal(0, context.SaveChangesCallCount);
        Assert.Empty(await context.Draws.ToListAsync());
    }

    [Fact]
    public async Task Handle_ShouldPersistSeparateDraws_ForConsecutiveCommands()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        var first = await HandleAsync(handler, new MakeDrawCommand(Creator, 4));
        var second = await HandleAsync(handler, new MakeDrawCommand("Başka Kullanıcı", 4));

        Assert.NotEqual(first.DrawId, second.DrawId);
        Assert.Equal(2, await context.Draws.CountAsync());
        Assert.Equal(8, await context.DrawGroups.CountAsync());
    }

    [Fact]
    public async Task Handle_ShouldReturnEveryTeamAsNamedObject()
    {
        // Beklenen çıktı biçimi: "teams":[{"name":"Adesso İstanbul"}, ...]
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        var response = await HandleAsync(handler, new MakeDrawCommand(Creator, 4));

        Assert.All(response.Groups, group =>
            Assert.All(group.Teams, team => Assert.False(string.IsNullOrWhiteSpace(team.Name))));
    }

    [Fact]
    public async Task Handle_Response_ShouldSerializeToTheSpecifiedJsonShape()
    {
        // Beklenen: {"groups":[{"groupName":"A","teams":[{"name":"Adesso İstanbul"}, ...]}, ...]}
        using var context = TestDbContextFactory.Create();
        var handler = new MakeDrawCommandHandler(context);

        var response = await HandleAsync(handler, new MakeDrawCommand(Creator, 4));

        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        using var document = JsonDocument.Parse(json);
        var groups = document.RootElement.GetProperty("groups");

        Assert.Equal(4, groups.GetArrayLength());
        Assert.Equal("A", groups[0].GetProperty("groupName").GetString());

        var teams = groups[0].GetProperty("teams");
        Assert.Equal(8, teams.GetArrayLength());
        Assert.All(teams.EnumerateArray(),
            team => Assert.False(string.IsNullOrWhiteSpace(team.GetProperty("name").GetString())));
    }

    private static Task<MakeDrawResponse> HandleAsync(
        MakeDrawCommandHandler handler,
        MakeDrawCommand command)
        => handler.Handle(command, CancellationToken.None);
}
