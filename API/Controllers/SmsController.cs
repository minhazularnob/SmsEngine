using Microsoft.AspNetCore.Mvc;
using SmsEngine.Application.Interfaces;
using SmsEngine.Domain.Entities;
using SmsEngine.Application.DTOs.Requests;

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
    public async Task<IActionResult> SendSms([FromBody] SingleSmsMessageRequest request)
    {
        try
        {

            var result = await _smsService.SendSmsAsync(request);
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
            var result = await _smsService.SendBulkSmsAsync(request);
            return result.Success
                ? Ok(new { Success = true, ReferenceIds = result.ReferenceId, Message = result.Message })
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
            var result = await _smsService.SendDynamicSmsAsync(request);
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
