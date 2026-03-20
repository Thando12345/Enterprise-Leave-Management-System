using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveFlow.Application.DTOs;
using LeaveFlow.Application.Features.LeaveRequests.Commands;
using LeaveFlow.Application.Features.LeaveRequests.Queries;
using LeaveFlow.Application.Interfaces;

namespace LeaveFlow.API.Controllers;

[ApiController]
[Route("api/leaverequests")]
[Authorize]
public class LeaveRequestsController(IMediator mediator, IIdempotencyRepository idempotency) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue("UserId")!);
    private int? CurrentTeamId => int.TryParse(User.FindFirstValue("TeamId"), out var t) ? t : null;

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await mediator.Send(new GetMyLeaveRequestsQuery(CurrentUserId));
        return Ok(result.Value);
    }

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances([FromQuery] int year = 0)
    {
        var result = await mediator.Send(new GetLeaveBalancesQuery(CurrentUserId, year == 0 ? DateTime.UtcNow.Year : year));
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (idempotencyKey is not null)
        {
            var cached = await idempotency.GetAsync(idempotencyKey);
            if (cached is not null) return Ok(cached.Response);
        }

        var result = await mediator.Send(new CreateLeaveRequestCommand(CurrentUserId, dto));
        if (!result.IsSuccess) return BadRequest(result.Error);

        if (idempotencyKey is not null)
            await idempotency.AddAsync(new Domain.Entities.IdempotencyKey { Key = idempotencyKey, Response = result.Value.ToString()! });

        return CreatedAtAction(nameof(GetMy), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await mediator.Send(new CancelLeaveRequestCommand(id, CurrentUserId));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> GetPending()
    {
        if (CurrentTeamId is null) return BadRequest("No team assigned.");
        var result = await mediator.Send(new GetPendingTeamRequestsQuery(CurrentTeamId.Value));
        return Ok(result.Value);
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewLeaveRequestDto dto)
    {
        var result = await mediator.Send(new ReviewLeaveRequestCommand(id, CurrentUserId, dto.Approve, dto.Comments));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
