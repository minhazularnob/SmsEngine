namespace SmsEngine.Application.DTOs.Requests;

public class SingleSmsMessageRequest
{
    public required string PhoneNumber { get; set; }
    public required string Message { get; set; }
}

public class BulkSmsRequest
{
    public required IEnumerable<Recipient> Recipients { get; set; }
    public string Message { get; set; }
}

public class DynamicSmsRequest
{
    public required IEnumerable<DynamicMessage> Messages { get; set; }
}

public class Recipient
{
    public required string PhoneNumber { get; set; }
}

public class DynamicMessage
{
    public required string PhoneNumber { get; set; }
    public required string Message { get; set; }
}
