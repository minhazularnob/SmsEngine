using SmsEngine.Application.DTOs;
using SmsEngine.Application.DTOs.Requests;
using SmsEngine.Domain.Entities;

namespace SmsEngine.Application.Interfaces;

public interface ISmsService
{
    Task<SmsResult> SendSmsAsync(SingleSmsMessageRequest message);
    Task<BulkSmsResult> SendBulkSmsAsync(BulkSmsRequest messages);
    Task<DynamicSmsResult> SendDynamicSmsAsync(DynamicSmsRequest messages);
 
}
