using Microsoft.AspNetCore.Mvc;
using SmsEngine.Application.Interfaces;
using SmsEngine.Domain.Entities;

namespace SmsEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmsController : ControllerBase
{
    private readonly ISmsService _smsService;
    private readonly ILogger<SmsController> _logger;

    public SmsController(ISmsService smsService, ILogger<SmsController> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    [HttpPost("single")]
    public async Task<IActionResult> SendSms([FromBody] SmsMessageRequest request)
    {
        try
        {
            var message = new SmsMessage
            {
                To = request.PhoneNumber,
                Body = request.Message,
                ReferenceId = request.ReferenceId
            };

            var result = await _smsService.SendSmsAsync(message);
            return result.Success 
                ? Ok(new { Success = true, ReferenceId = result.ReferenceId, Message = result.Message })
                : BadRequest(new { Success = false, result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new { Success = false, Message = "An error occurred while sending SMS" });
        }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> SendBulkSms([FromBody] BulkSmsRequest request)
    {
        try
        {
            var messages = request.Recipients.Select(r => new SmsMessage
            {
                To = r.PhoneNumber,
                Body = r.Message ?? request.Message,
                ReferenceId = r.ReferenceId
            });

            var result = await _smsService.SendBulkSmsAsync(messages);
            return result.Success
                ? Ok(new { Success = true, ReferenceIds = result.ReferenceIds, Message = result.Message })
                : BadRequest(new { Success = false, result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk SMS");
            return StatusCode(500, new { Success = false, Message = "An error occurred while sending bulk SMS" });
        }
    }

    [HttpPost("dynamic")]
    public async Task<IActionResult> SendDynamicSms([FromBody] DynamicSmsRequest request)
    {
        try
        {
            var messages = request.Messages.ToDictionary(
                m => m.PhoneNumber,
                m => new SmsMessage
                {
                    To = m.PhoneNumber,
                    Body = m.Message,
                    ReferenceId = m.ReferenceId
                }
            );

            var result = await _smsService.SendDynamicSmsAsync(messages);
            return result.Success
                ? Ok(new { Success = true, ReferenceIds = result.ReferenceIds, Message = result.Message })
                : BadRequest(new { Success = false, result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending dynamic SMS");
            return StatusCode(500, new { Success = false, Message = "An error occurred while sending dynamic SMS" });
        }
    }
}

// Request DTOs
public class SmsMessageRequest
{
    public required string PhoneNumber { get; set; }
    public required string Message { get; set; }
    public string? ReferenceId { get; set; }
}

public class BulkSmsRequest
{
    public required IEnumerable<Recipient> Recipients { get; set; }
    public string? Message { get; set; }
}

public class DynamicSmsRequest
{
    public required IEnumerable<DynamicMessage> Messages { get; set; }
}

public class Recipient
{
    public required string PhoneNumber { get; set; }
    public string? Message { get; set; }
    public string? ReferenceId { get; set; }
}

public class DynamicMessage
{
    public required string PhoneNumber { get; set; }
    public required string Message { get; set; }
    public string? ReferenceId { get; set; }
}
