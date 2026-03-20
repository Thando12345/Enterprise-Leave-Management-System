using MediatR;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.DTOs;
using LeaveFlow.Application.Interfaces;

namespace LeaveFlow.Application.Features.LeaveRequests.Queries;

public record GetMyLeaveRequestsQuery(int EmployeeId) : IRequest<Result<IEnumerable<LeaveRequestDto>>>;

public class GetMyLeaveRequestsHandler(ILeaveRequestRepository requests)
    : IRequestHandler<GetMyLeaveRequestsQuery, Result<IEnumerable<LeaveRequestDto>>>
{
    public async Task<Result<IEnumerable<LeaveRequestDto>>> Handle(GetMyLeaveRequestsQuery query, CancellationToken ct)
    {
        var list = await requests.GetByEmployeeAsync(query.EmployeeId);
        var dtos = list.Select(r => new LeaveRequestDto(
            r.Id, r.EmployeeId, r.Employee?.FullName ?? string.Empty,
            r.StartDate, r.EndDate, r.LeaveType, r.Status, r.RequestDate, r.Comments, r.TotalDays));
        return Result<IEnumerable<LeaveRequestDto>>.Success(dtos);
    }
}

public record GetPendingTeamRequestsQuery(int TeamId) : IRequest<Result<IEnumerable<LeaveRequestDto>>>;

public class GetPendingTeamRequestsHandler(ILeaveRequestRepository requests)
    : IRequestHandler<GetPendingTeamRequestsQuery, Result<IEnumerable<LeaveRequestDto>>>
{
    public async Task<Result<IEnumerable<LeaveRequestDto>>> Handle(GetPendingTeamRequestsQuery query, CancellationToken ct)
    {
        var list = await requests.GetPendingByTeamAsync(query.TeamId);
        var dtos = list.Select(r => new LeaveRequestDto(
            r.Id, r.EmployeeId, r.Employee?.FullName ?? string.Empty,
            r.StartDate, r.EndDate, r.LeaveType, r.Status, r.RequestDate, r.Comments, r.TotalDays));
        return Result<IEnumerable<LeaveRequestDto>>.Success(dtos);
    }
}

public record GetLeaveBalancesQuery(int EmployeeId, int Year) : IRequest<Result<IEnumerable<LeaveBalanceDto>>>;

public class GetLeaveBalancesHandler(ILeaveBalanceRepository balances)
    : IRequestHandler<GetLeaveBalancesQuery, Result<IEnumerable<LeaveBalanceDto>>>
{
    public async Task<Result<IEnumerable<LeaveBalanceDto>>> Handle(GetLeaveBalancesQuery query, CancellationToken ct)
    {
        var list = await balances.GetByEmployeeAsync(query.EmployeeId, query.Year);
        var dtos = list.Select(b => new LeaveBalanceDto(b.LeaveType, b.TotalDays, b.UsedDays, b.RemainingDays));
        return Result<IEnumerable<LeaveBalanceDto>>.Success(dtos);
    }
}
