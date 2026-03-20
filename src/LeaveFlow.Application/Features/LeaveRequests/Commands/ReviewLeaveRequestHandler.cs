using MediatR;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Interfaces;
using LeaveFlow.Domain.Entities;
using LeaveFlow.Domain.Enums;

namespace LeaveFlow.Application.Features.LeaveRequests.Commands;

public record ReviewLeaveRequestCommand(int RequestId, int ManagerId, bool Approve, string? Comments) : IRequest<Result<bool>>;

public class ReviewLeaveRequestHandler(
    ILeaveRequestRepository requests,
    ILeaveBalanceRepository balances,
    IAuditLogRepository audit,
    IEmailService email,
    IUserRepository users,
    IUnitOfWork uow) : IRequestHandler<ReviewLeaveRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ReviewLeaveRequestCommand cmd, CancellationToken ct)
    {
        var request = await requests.GetByIdAsync(cmd.RequestId);
        if (request is null) return Result<bool>.Failure("Request not found.");
        if (request.Status != LeaveStatus.Pending) return Result<bool>.Failure("Request is not pending.");

        request.Status = cmd.Approve ? LeaveStatus.Approved : LeaveStatus.Rejected;
        request.ReviewedBy = cmd.ManagerId;
        request.ReviewDate = DateTime.UtcNow;
        request.Comments = cmd.Comments ?? request.Comments;

        if (cmd.Approve)
        {
            var balance = await balances.GetAsync(request.EmployeeId, request.LeaveType, request.StartDate.Year);
            if (balance is not null)
            {
                balance.UsedDays += request.TotalDays;
                await balances.UpdateAsync(balance);
            }
        }

        await requests.UpdateAsync(request);
        await audit.AddAsync(new AuditLog { UserId = cmd.ManagerId, Action = cmd.Approve ? "ApproveLeave" : "RejectLeave", Details = $"RequestId:{cmd.RequestId}" });
        await uow.SaveChangesAsync(ct);

        var employee = await users.GetByIdAsync(request.EmployeeId);
        if (employee is not null)
        {
            var status = cmd.Approve ? "approved" : "rejected";
            await email.SendAsync(employee.Email, $"Leave Request {status}", $"Your leave request has been {status}.");
        }

        return Result<bool>.Success(true);
    }
}
