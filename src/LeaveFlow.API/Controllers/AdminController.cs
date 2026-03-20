using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveFlow.Application.Interfaces;

namespace LeaveFlow.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IAuditLogRepository audit) : ControllerBase
{
    [HttpGet("auditlogs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var logs = await audit.GetAllAsync(page, pageSize);
        return Ok(logs);
    }
}
