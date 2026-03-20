namespace LeaveFlow.Domain.Entities;

public class IdempotencyKey
{
    public string Key { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
