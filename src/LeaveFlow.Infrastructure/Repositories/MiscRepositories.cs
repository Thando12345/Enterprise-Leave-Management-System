using Microsoft.EntityFrameworkCore;
using LeaveFlow.Application.Interfaces;
using LeaveFlow.Domain.Entities;
using LeaveFlow.Infrastructure.Persistence;

namespace LeaveFlow.Infrastructure.Repositories;

public class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log) => await db.AuditLogs.AddAsync(log);
    public async Task<IEnumerable<AuditLog>> GetAllAsync(int page, int pageSize) =>
        await db.AuditLogs.OrderByDescending(l => l.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
}

public class IdempotencyRepository(AppDbContext db) : IIdempotencyRepository
{
    public Task<IdempotencyKey?> GetAsync(string key) => db.IdempotencyKeys.FindAsync(key).AsTask();
    public async Task AddAsync(IdempotencyKey entry) => await db.IdempotencyKeys.AddAsync(entry);
}
