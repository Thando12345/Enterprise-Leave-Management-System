using LeaveFlow.Domain.Enums;

namespace LeaveFlow.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, string Role, string FullName);

public record CreateLeaveRequestDto(
    DateOnly StartDate,
    DateOnly EndDate,
    LeaveType LeaveType,
    string? Comments);

public record LeaveRequestDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    DateOnly StartDate,
    DateOnly EndDate,
    LeaveType LeaveType,
    LeaveStatus Status,
    DateTime RequestDate,
    string? Comments,
    int TotalDays);

public record ReviewLeaveRequestDto(bool Approve, string? Comments);

public record LeaveBalanceDto(LeaveType LeaveType, int TotalDays, int UsedDays, int RemainingDays);
