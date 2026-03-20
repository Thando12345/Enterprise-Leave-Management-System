using LeaveFlow.Domain.Enums;

namespace LeaveFlow.Domain.Entities;

public class LeaveBalance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public User Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays => TotalDays - UsedDays;
    public int Year { get; set; } = DateTime.UtcNow.Year;
}
