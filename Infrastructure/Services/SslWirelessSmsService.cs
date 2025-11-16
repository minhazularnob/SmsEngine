using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmsEngine.Application.DTOs;
using SmsEngine.Application.Interfaces;
using SmsEngine.Domain.Entities;
using SmsEngine.Infrastructure.Configuration;

namespace SmsEngine.Infrastructure.Services;

public class SslWirelessSmsService : ISmsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SslWirelessSmsService> _logger;
    private readonly SmsSettings _smsSettings;

    public SslWirelessSmsService(
        IHttpClientFactory httpClientFactory,
        IOptions<SmsSettings> smsSettings,
        ILogger<SslWirelessSmsService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _smsSettings = smsSettings.Value;
    }

    public async Task<SmsResult> SendSmsAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                msisdn = message.To,
                sms = message.Body,
                csms_id = message.ReferenceId ?? $"sms_{DateTime.Now:yyyyMMdd}"
            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

            return new SmsResult(true, "SMS sent successfully", request.csms_id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", message.To);
            return new SmsResult(false, ex.Message);
        }
    }

    public async Task<BulkSmsResult> SendBulkSmsAsync(IEnumerable<SmsMessage> messages, CancellationToken cancellationToken = default)
    {
        var referenceIds = new List<string>();
        var batchId = $"bulk_{DateTime.Now:yyyyMMddHHmmss}";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                sms = messages.Select((m, i) => new
                {
                    msisdn = m.To,
                    sms = m.Body,
                    csms_id = m.ReferenceId ?? $"{batchId}_{i:D4}"
                }),
                batch_csms_id = batchId
            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms/bulk", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            referenceIds = request.sms.Select(x => x.csms_id).ToList();
            return new BulkSmsResult(true, referenceIds, "Bulk SMS sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk SMS");
            return new BulkSmsResult(false, referenceIds, ex.Message);
        }
    }

    public async Task<DynamicSmsResult> SendDynamicSmsAsync(Dictionary<string, SmsMessage> messages, CancellationToken cancellationToken = default)
    {
        var referenceIds = new Dictionary<string, string>();
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        try
        {
            var client = _httpClientFactory.CreateClient();
            var smsList = messages.Select((kvp, i) => new
            {
                msisdn = kvp.Key,
                text = kvp.Value.Body,
                csms_id = $"dyn_{timestamp}_{i:D4}"
            }).ToList();

            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                sms = smsList
            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms/dynamic", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            referenceIds = smsList.ToDictionary(
                x => x.msisdn,
                x => x.csms_id
            );

            return new DynamicSmsResult(true, referenceIds, "Dynamic SMS sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending dynamic SMS");
            return new DynamicSmsResult(false, referenceIds, ex.Message);
        }
    }
}
