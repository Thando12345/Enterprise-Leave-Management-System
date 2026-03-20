using LeaveFlow.Domain.Enums;

namespace LeaveFlow.Domain.Entities;

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public User Employee { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public LeaveType LeaveType { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? Comments { get; set; }

    public int TotalDays => EndDate.DayNumber - StartDate.DayNumber + 1;
}
