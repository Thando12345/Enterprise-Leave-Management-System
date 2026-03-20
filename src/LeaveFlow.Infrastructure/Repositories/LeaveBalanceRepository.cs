using Microsoft.EntityFrameworkCore;
using LeaveFlow.Application.Interfaces;
using LeaveFlow.Domain.Entities;
using LeaveFlow.Domain.Enums;
using LeaveFlow.Infrastructure.Persistence;

namespace LeaveFlow.Infrastructure.Repositories;

public class LeaveBalanceRepository(AppDbContext db) : ILeaveBalanceRepository
{
    public Task<LeaveBalance?> GetAsync(int employeeId, LeaveType type, int year) =>
        db.LeaveBalances.FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveType == type && b.Year == year);

    public async Task<IEnumerable<LeaveBalance>> GetByEmployeeAsync(int employeeId, int year) =>
        await db.LeaveBalances.Where(b => b.EmployeeId == employeeId && b.Year == year).ToListAsync();

    public async Task AddAsync(LeaveBalance balance) => await db.LeaveBalances.AddAsync(balance);
    public Task UpdateAsync(LeaveBalance balance) { db.LeaveBalances.Update(balance); return Task.CompletedTask; }
}
