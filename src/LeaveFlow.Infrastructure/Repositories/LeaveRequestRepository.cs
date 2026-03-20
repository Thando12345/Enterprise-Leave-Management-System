using Microsoft.EntityFrameworkCore;
using LeaveFlow.Application.Interfaces;
using LeaveFlow.Domain.Entities;
using LeaveFlow.Domain.Enums;
using LeaveFlow.Infrastructure.Persistence;

namespace LeaveFlow.Infrastructure.Repositories;

public class LeaveRequestRepository(AppDbContext db) : ILeaveRequestRepository
{
    public Task<LeaveRequest?> GetByIdAsync(int id) =>
        db.LeaveRequests.Include(r => r.Employee).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId) =>
        await db.LeaveRequests.Include(r => r.Employee).Where(r => r.EmployeeId == employeeId).ToListAsync();

    public async Task<IEnumerable<LeaveRequest>> GetPendingByTeamAsync(int teamId) =>
        await db.LeaveRequests.Include(r => r.Employee)
            .Where(r => r.Status == LeaveStatus.Pending && r.Employee.TeamId == teamId).ToListAsync();

    public Task<bool> HasOverlapAsync(int employeeId, DateOnly start, DateOnly end, int? excludeId = null) =>
        db.LeaveRequests.AnyAsync(r =>
            r.EmployeeId == employeeId &&
            r.Id != excludeId &&
            r.Status != LeaveStatus.Canceled &&
            r.Status != LeaveStatus.Rejected &&
            r.StartDate <= end && r.EndDate >= start);

    public async Task AddAsync(LeaveRequest request) => await db.LeaveRequests.AddAsync(request);
    public Task UpdateAsync(LeaveRequest request) { db.LeaveRequests.Update(request); return Task.CompletedTask; }
}
