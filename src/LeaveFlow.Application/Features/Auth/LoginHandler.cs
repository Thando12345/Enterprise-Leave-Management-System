using MediatR;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.DTOs;
using LeaveFlow.Application.Interfaces;

namespace LeaveFlow.Application.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public class LoginHandler(IUserRepository users, IJwtService jwt, IPasswordHasher hasher)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(cmd.Email);
        if (user is null || !hasher.Verify(cmd.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid credentials.");

        var token = jwt.GenerateToken(user);
        var refresh = jwt.GenerateRefreshToken();
        return Result<LoginResponse>.Success(new LoginResponse(token, refresh, user.Role.ToString(), user.FullName));
    }
}
