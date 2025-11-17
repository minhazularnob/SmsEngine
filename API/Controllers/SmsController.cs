using Microsoft.AspNetCore.Mvc;
using SmsEngine.Application.DTOs;
using SmsEngine.Application.DTOs.Requests;
using SmsEngine.Application.Interfaces;
using Serilog;

namespace SmsEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmsController : ControllerBase
{
    private readonly ISmsService _smsService;

    public SmsController(ISmsService smsService, ILogger<SmsController> logger)
    {
        _smsService = smsService;
    }
       
    [HttpPost("single")]
    public async Task<ActionResult<SingleSmsResult>> SendSingleSms([FromBody] SingleSmsMessageRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _smsService.SendSingleSmsAsync(request);

            if (!result.Success)
            {
                // FAILED means bad request (invalid number, blocked, insufficient balance etc.)
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while sending SMS to {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new SingleSmsResult
            {
                Success = false,
                Status = "ERROR",
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkSmsResult>> SendBulkSms([FromBody] BulkSmsRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _smsService.SendBulkSmsAsync(request);

            // Partial success/failure: always return 200, include details in Items
            return Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while sending bulk SMS");
            var batchId = $"bulk_{Guid.NewGuid():N}";

            var errorResult = new BulkSmsResult
            {
                Status = "FAILED",
                Success = false,
                Message = "Internal server error while sending bulk SMS",
                BatchId = batchId,
                Items = request.Recipients.Select(r => new BulkSmsItem
                {
                    PhoneNumber = r.PhoneNumber,
                    Status = "ERROR",
                    Message = ex.Message
                }).ToList()
            };

            return StatusCode(500, errorResult);
        }
    }

    [HttpPost("dynamic")]
    public async Task<ActionResult<DynamicSmsResult>> SendDynamicSms([FromBody] DynamicSmsRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _smsService.SendDynamicSmsAsync(request);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending dynamic SMS");

            var batchId = $"dyn_{Guid.NewGuid():N}";

            var errorResult = new DynamicSmsResult
            {
                Status = "FAILED",
                Message = "Internal server error while sending dynamic SMS",
                Items = request.Messages.Select((m, i) => new DynamicSmsItem
                {
                    PhoneNumber = m.PhoneNumber,
                    CsmsId = $"d{batchId}_{i:D4}",
                    SmsStatus = "ERROR",
                    StatusMessage = ex.Message
                }).ToList()
            };

            return StatusCode(500, errorResult);
        }
    }
}
