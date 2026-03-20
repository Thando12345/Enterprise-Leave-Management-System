using LeaveFlow.Application.Interfaces;
using LeaveFlow.Infrastructure.Persistence;

namespace LeaveFlow.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
