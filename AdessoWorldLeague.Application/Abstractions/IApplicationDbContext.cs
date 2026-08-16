using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdessoWorldLeague.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Team> Teams { get; }
    DbSet<Draw> Draws { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
