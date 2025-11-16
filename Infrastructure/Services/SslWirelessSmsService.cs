using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmsEngine.Application.DTOs;
using SmsEngine.Application.DTOs.Requests;
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

    public async Task<SmsResult> SendSmsAsync(SingleSmsMessageRequest message)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                msisdn = message.PhoneNumber,
                sms = message.Message,
                csms_id = $"sms_{DateTime.Now:yyyyMMdd}"
            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms", request);
            response.EnsureSuccessStatusCode();

            return new SmsResult(true, "SMS sent successfully", request.csms_id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", message.PhoneNumber);
            return new SmsResult(false, ex.Message);
        }
    }

    public async Task<BulkSmsResult> SendBulkSmsAsync(BulkSmsRequest messages)
    {
        string referenceId = null;
        var batchId = $"bulk_{DateTime.Now:yyyyMMddHHmmss}";

        try
        {
            var client = _httpClientFactory.CreateClient();

            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                sms= messages.Message,
                batch_csms_id = batchId,
                msisdn = messages.Recipients.ToList()

            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms/bulk", request);
            response.EnsureSuccessStatusCode();

            referenceId = request.batch_csms_id;
            return new BulkSmsResult(true, referenceId.ToString(), "Bulk SMS sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk SMS");
            return new BulkSmsResult(false, referenceId.ToString(), ex.Message);
        }
    }

    public async Task<DynamicSmsResult> SendDynamicSmsAsync(DynamicSmsRequest messages)
    {
        var referenceIds = new Dictionary<string, string>();
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        try
        {
            var client = _httpClientFactory.CreateClient();

            var smsList = messages.Messages
             .Select((msg, i) => new
             {
                 msisdn = msg.PhoneNumber,
                 text = msg.Message,
                 csms_id = $"d{timestamp}{i:D4}" // 16 char max
             }).ToList();

            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                sms = smsList
            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms/dynamic", request);
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
