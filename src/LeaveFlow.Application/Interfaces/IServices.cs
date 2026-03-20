using LeaveFlow.Domain.Entities;

namespace LeaveFlow.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}
