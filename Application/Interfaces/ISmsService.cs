using SmsEngine.Application.DTOs;
using SmsEngine.Domain.Entities;

namespace SmsEngine.Application.Interfaces;

public interface ISmsService
{
    Task<SmsResult> SendSmsAsync(SmsMessage message, CancellationToken cancellationToken = default);
    Task<BulkSmsResult> SendBulkSmsAsync(IEnumerable<SmsMessage> messages, CancellationToken cancellationToken = default);
    Task<DynamicSmsResult> SendDynamicSmsAsync(Dictionary<string, SmsMessage> messages, CancellationToken cancellationToken = default);
}
