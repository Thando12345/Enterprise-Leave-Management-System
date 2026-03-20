using Microsoft.EntityFrameworkCore;
using LeaveFlow.Domain.Entities;

namespace LeaveFlow.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<IdempotencyKey>().HasKey(k => k.Key);
        mb.Entity<LeaveBalance>().HasIndex(b => new { b.EmployeeId, b.LeaveType, b.Year }).IsUnique();
    }
}
