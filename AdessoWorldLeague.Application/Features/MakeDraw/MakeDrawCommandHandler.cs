using AdessoWorldLeague.Domain.Entities;
using AdessoWorldLeague.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Application.Features.MakeDraw;

public class MakeDrawCommandHandler(AppDbContext dbContext) : IRequestHandler<MakeDrawCommand, MakeDrawResponse>
{
    public async Task<MakeDrawResponse> Handle(MakeDrawCommand request, CancellationToken cancellationToken)
    {
        var teams = await dbContext.Teams
            .Include(t => t.Country)
            .ToListAsync(cancellationToken);

        var draw = new Draw(request.CreatorFullName);
        draw.Make(teams, request.GroupCount);

        dbContext.Draws.Add(draw);
        await dbContext.SaveChangesAsync(cancellationToken);

        var groups = draw.DrawGroups.Select(g => new DrawGroupDto(
            g.GroupName,
            g.DrawTeamAssignments
                .Select(a => teams.First(t => t.Id == a.TeamId).Name)
                .ToList()
        )).ToList();

        return new MakeDrawResponse(draw.Id, groups);
    }
}
