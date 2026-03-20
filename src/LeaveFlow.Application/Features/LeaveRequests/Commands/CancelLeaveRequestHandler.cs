using MediatR;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Interfaces;
using LeaveFlow.Domain.Entities;
using LeaveFlow.Domain.Enums;

namespace LeaveFlow.Application.Features.LeaveRequests.Commands;

public record CancelLeaveRequestCommand(int RequestId, int EmployeeId) : IRequest<Result<bool>>;

public class CancelLeaveRequestHandler(
    ILeaveRequestRepository requests,
    IAuditLogRepository audit,
    IUnitOfWork uow) : IRequestHandler<CancelLeaveRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelLeaveRequestCommand cmd, CancellationToken ct)
    {
        var request = await requests.GetByIdAsync(cmd.RequestId);
        if (request is null || request.EmployeeId != cmd.EmployeeId) return Result<bool>.Failure("Request not found.");
        if (request.Status != LeaveStatus.Pending) return Result<bool>.Failure("Only pending requests can be canceled.");

        request.Status = LeaveStatus.Canceled;
        await requests.UpdateAsync(request);
        await audit.AddAsync(new AuditLog { UserId = cmd.EmployeeId, Action = "CancelLeaveRequest", Details = $"RequestId:{cmd.RequestId}" });
        await uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
