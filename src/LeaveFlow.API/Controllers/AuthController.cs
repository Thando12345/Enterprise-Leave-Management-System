using MediatR;
using Microsoft.AspNetCore.Mvc;
using LeaveFlow.Application.DTOs;
using LeaveFlow.Application.Features.Auth;

namespace LeaveFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password));
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(result.Error);
    }
}
