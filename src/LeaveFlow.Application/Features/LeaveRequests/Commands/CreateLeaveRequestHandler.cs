using MediatR;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.DTOs;
using LeaveFlow.Application.Interfaces;
using LeaveFlow.Domain.Entities;
using LeaveFlow.Domain.Enums;

namespace LeaveFlow.Application.Features.LeaveRequests.Commands;

public record CreateLeaveRequestCommand(int EmployeeId, CreateLeaveRequestDto Dto) : IRequest<Result<int>>;

public class CreateLeaveRequestHandler(
    ILeaveRequestRepository requests,
    ILeaveBalanceRepository balances,
    IAuditLogRepository audit,
    IEmailService email,
    IUserRepository users,
    IUnitOfWork uow) : IRequestHandler<CreateLeaveRequestCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateLeaveRequestCommand cmd, CancellationToken ct)
    {
        var dto = cmd.Dto;
        if (dto.EndDate < dto.StartDate)
            return Result<int>.Failure("End date must be after start date.");

        if (await requests.HasOverlapAsync(cmd.EmployeeId, dto.StartDate, dto.EndDate))
            return Result<int>.Failure("Overlapping leave request exists.");

        var balance = await balances.GetAsync(cmd.EmployeeId, dto.LeaveType, dto.StartDate.Year);
        var days = dto.EndDate.DayNumber - dto.StartDate.DayNumber + 1;
        if (balance is null || balance.RemainingDays < days)
            return Result<int>.Failure("Insufficient leave balance.");

        var request = new LeaveRequest
        {
            EmployeeId = cmd.EmployeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            LeaveType = dto.LeaveType,
            Comments = dto.Comments
        };
        await requests.AddAsync(request);
        await audit.AddAsync(new AuditLog { UserId = cmd.EmployeeId, Action = "CreateLeaveRequest", Details = $"Type:{dto.LeaveType} Days:{days}" });
        await uow.SaveChangesAsync(ct);

        var user = await users.GetByIdAsync(cmd.EmployeeId);
        if (user is not null)
            await email.SendAsync(user.Email, "Leave Request Submitted", $"Your {dto.LeaveType} leave request has been submitted.");

        return Result<int>.Success(request.Id);
    }
}
