namespace SmsEngine.Domain.Entities;

public class SmsMessage
{
    public string To { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public SmsStatus Status { get; set; } = SmsStatus.Pending;
}

public enum SmsStatus
{
    Pending,
    Sent,
    Delivered,
    Failed
}
