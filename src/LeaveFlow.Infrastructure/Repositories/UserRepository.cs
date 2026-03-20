using Microsoft.EntityFrameworkCore;
using LeaveFlow.Application.Interfaces;
using LeaveFlow.Domain.Entities;
using LeaveFlow.Infrastructure.Persistence;

namespace LeaveFlow.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) => db.Users.FirstOrDefaultAsync(u => u.Email == email);
    public Task<User?> GetByIdAsync(int id) => db.Users.FindAsync(id).AsTask();
    public async Task<IEnumerable<User>> GetByTeamAsync(int teamId) => await db.Users.Where(u => u.TeamId == teamId).ToListAsync();
    public async Task AddAsync(User user) => await db.Users.AddAsync(user);
    public Task UpdateAsync(User user) { db.Users.Update(user); return Task.CompletedTask; }
}
