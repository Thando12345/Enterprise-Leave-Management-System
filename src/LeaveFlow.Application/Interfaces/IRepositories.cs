using LeaveFlow.Domain.Entities;
using LeaveFlow.Domain.Enums;

namespace LeaveFlow.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetByTeamAsync(int teamId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetPendingByTeamAsync(int teamId);
    Task<bool> HasOverlapAsync(int employeeId, DateOnly start, DateOnly end, int? excludeId = null);
    Task AddAsync(LeaveRequest request);
    Task UpdateAsync(LeaveRequest request);
}

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetAsync(int employeeId, LeaveType type, int year);
    Task<IEnumerable<LeaveBalance>> GetByEmployeeAsync(int employeeId, int year);
    Task AddAsync(LeaveBalance balance);
    Task UpdateAsync(LeaveBalance balance);
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<IEnumerable<AuditLog>> GetAllAsync(int page, int pageSize);
}

public interface IIdempotencyRepository
{
    Task<IdempotencyKey?> GetAsync(string key);
    Task AddAsync(IdempotencyKey entry);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
