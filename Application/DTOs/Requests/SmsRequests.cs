using System.ComponentModel.DataAnnotations;

namespace SmsEngine.Application.DTOs.Requests;

public class SingleSmsMessageRequest
{
    [Required(ErrorMessage = "PhoneNumber is required")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Message is required")]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; } = null!;
}

public class BulkSmsRequest
{
    public required IEnumerable<Recipient> Recipients { get; set; }
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; }
}
public class Recipient
{
    [Required(ErrorMessage = "PhoneNumber is required")]
    public required string PhoneNumber { get; set; }
}

public class DynamicSmsRequest
{
    public required IEnumerable<DynamicMessage> Messages { get; set; }
}



public class DynamicMessage
{
    [Required(ErrorMessage = "PhoneNumber is required")]
    public required string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public required string Message { get; set; }
}
