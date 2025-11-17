using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmsEngine.Application.DTOs;
using SmsEngine.Application.DTOs.Requests;
using SmsEngine.Application.Interfaces;
using SmsEngine.Infrastructure.Configuration;
using Serilog;

namespace SmsEngine.Infrastructure.Services;

public class SslWirelessSmsService : ISmsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SmsSettings _smsSettings;

    public SslWirelessSmsService(IHttpClientFactory httpClientFactory, IOptions<SmsSettings> smsSettings)
    {
        _httpClientFactory = httpClientFactory;
        _smsSettings = smsSettings.Value;
    }

    // 1. Update SendSmsAsync
    public async Task<SingleSmsResult> SendSingleSmsAsync(SingleSmsMessageRequest message)
    {
        var csmsId = $"sms_{DateTime.Now:yyyyMMdd}";

        try
        {
            var client = _httpClientFactory.CreateClient();

            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                msisdn = message.PhoneNumber,
                sms = message.Message,
                csms_id = csmsId
            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms", request);
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            // ⬇ Unified JSON Parsing
            var result = new
            {
                Status = root.GetProperty("status").GetString(),
                StatusCode = root.GetProperty("status_code").GetInt32(),
                ErrorMessage = root.GetProperty("error_message").GetString(),

                SmsInfo = root.TryGetProperty("smsinfo", out var smsInfoElement) &&
                          smsInfoElement.GetArrayLength() > 0
                            ? smsInfoElement[0]
                            : (JsonElement?)null
            };

            // Extract smsinfo safely
            string? smsStatus = result.SmsInfo?.GetProperty("sms_status").GetString();
            string? statusMsg = result.SmsInfo?.GetProperty("status_message").GetString();
            string? msisdn = result.SmsInfo?.GetProperty("msisdn").GetString();
            string? smsType = result.SmsInfo?.GetProperty("sms_type").GetString();
            string? smsBody = result.SmsInfo?.GetProperty("sms_body").GetString();
            string? referenceId = result.SmsInfo?.GetProperty("reference_id").GetString();

            bool isSuccess = result.Status == "SUCCESS" && result.StatusCode == 200;

            return new SingleSmsResult
            {
                Status = smsStatus ?? result.Status,
                Success = isSuccess,
                Message = isSuccess ? statusMsg ?? "SMS sent successfully" : result.ErrorMessage,
                ReferenceId = referenceId,
                PhoneNumber = msisdn ?? message.PhoneNumber,
                CsmsId = csmsId,
                SmsType = smsType,
                SmsBody = smsBody,
                StatusCode = result.StatusCode.ToString()
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending SMS to {PhoneNumber}", message.PhoneNumber);

            return new SingleSmsResult
            {
                Success = false,
                Message = ex.Message,
                Status = "ERROR",
                PhoneNumber = message.PhoneNumber,
                CsmsId = csmsId
            };
        }
    }

    public async Task<BulkSmsResult> SendBulkSmsAsync(BulkSmsRequest messages)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var batchId = $"bulk_{timestamp}";
        var items = new List<BulkSmsItem>();

        try
        {
            var client = _httpClientFactory.CreateClient();

            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                sms = messages.Message,
                batch_csms_id = batchId,
                msisdn = messages.Recipients.Select(r => r.PhoneNumber).ToList()
            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms/bulk", request);
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var status = root.TryGetProperty("status", out var sEl) ? sEl.GetString() : "FAILED";
            var statusCode = root.TryGetProperty("status_code", out var scEl) ? scEl.GetInt32() : 0;
            var errorMessage = root.TryGetProperty("error_message", out var emEl) ? emEl.GetString() ?? "Unknown error" : "Unknown error";

            if (root.TryGetProperty("smsinfo", out var smsInfoElement) && smsInfoElement.GetArrayLength() > 0)
            {
                foreach (var sms in smsInfoElement.EnumerateArray())
                {
                    items.Add(new BulkSmsItem
                    {
                        ReferenceId = sms.TryGetProperty("reference_id", out var r) ? r.GetString() : null,
                        Status = sms.TryGetProperty("sms_status", out var st) ? st.GetString() : "FAILED",
                        Message = sms.TryGetProperty("status_message", out var msg) ? msg.GetString() : errorMessage,
                        PhoneNumber = sms.TryGetProperty("msisdn", out var ph) ? ph.GetString() : null,
                        CsmsId = sms.TryGetProperty("csms_id", out var c) ? c.GetString() : null,
                        SmsType = sms.TryGetProperty("sms_type", out var type) ? type.GetString() : null,
                        SmsBody = sms.TryGetProperty("sms_body", out var body) ? body.GetString() : null
                    });
                }

                var successCount = items.Count(i => i.Status == "SUCCESS");
                return new BulkSmsResult
                {
                    Status = status,
                    Success = successCount > 0,
                    Message = $"Sent {successCount} of {items.Count} messages successfully",
                    BatchId = batchId,
                    Items = items
                };
            }

            // all failed
            items.AddRange(messages.Recipients.Select(r => new BulkSmsItem
            {
                Status = "FAILED",
                Message = errorMessage,
                PhoneNumber = r.PhoneNumber
            }));

            return new BulkSmsResult
            {
                Success = false,
                Message = $"Failed to send bulk SMS: {errorMessage}",
                BatchId = batchId,
                Items = items
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending bulk SMS");

            items.AddRange(messages.Recipients.Select(r => new BulkSmsItem
            {
                Status = "ERROR",
                Message = ex.Message,
                PhoneNumber = r.PhoneNumber
            }));

            return new BulkSmsResult
            {
                Success = false,
                Message = $"Error sending bulk SMS: {ex.Message}",
                BatchId = batchId,
                Items = items
            };
        }
    }

    // 3. Update SendDynamicSmsAsync
    public async Task<DynamicSmsResult> SendDynamicSmsAsync(DynamicSmsRequest messages)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var batchId = $"dyn_{timestamp}";
        var items = new List<DynamicSmsItem>();
        var messageList = messages.Messages.ToList();

        try
        {
            var client = _httpClientFactory.CreateClient();

            // Generate smsList with unique csms_id
            var smsList = messageList
                .Select((msg, i) => new
                {
                    msisdn = msg.PhoneNumber,
                    text = msg.Message,
                    csms_id = $"{batchId}_{i:D4}"
                })
                .ToList();

            var request = new
            {
                api_token = _smsSettings.ApiToken,
                sid = _smsSettings.Sid,
                sms = smsList
            };

            var response = await client.PostAsJsonAsync($"{_smsSettings.BaseUrl}/send-sms/dynamic", request);
            var responseContent = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var status = root.GetProperty("status").GetString() ?? "FAILED";
            var statusCode = root.GetProperty("status_code").GetInt32();
            var errorMessage = root.GetProperty("error_message").GetString() ?? string.Empty;

            if (root.TryGetProperty("smsinfo", out var smsInfoElement))
            {
                foreach (var sms in smsInfoElement.EnumerateArray())
                {
                    items.Add(new DynamicSmsItem
                    {
                        ReferenceId = sms.GetProperty("reference_id").GetString(),
                        SmsStatus = sms.GetProperty("sms_status").GetString(),
                        StatusMessage = sms.GetProperty("status_message").GetString(),
                        PhoneNumber = sms.GetProperty("msisdn").GetString(),
                        CsmsId = sms.GetProperty("csms_id").GetString(),
                        SmsType = sms.GetProperty("sms_type").GetString(),
                        SmsBody = sms.GetProperty("sms_body").GetString()
                    });
                }
            }

            // Determine success based on each item's status
            var successCount = items.Count(i => i.SmsStatus == "SUCCESS");
            var resultMessage = successCount > 0
                ? $"Sent {successCount} of {items.Count} dynamic messages successfully"
                : $"Failed to send dynamic SMS: {errorMessage}";

            // Handle any messages not returned in smsinfo
            var returnedPhones = items.Select(i => i.PhoneNumber).ToHashSet();
            for (int i = 0; i < messageList.Count; i++)
            {
                if (!returnedPhones.Contains(messageList[i].PhoneNumber))
                {
                    items.Add(new DynamicSmsItem
                    {
                        SmsStatus = "FAILED",
                        StatusMessage = errorMessage,
                        PhoneNumber = messageList[i].PhoneNumber,
                        CsmsId = $"{batchId}_{i:D4}",
                        SmsBody = messageList[i].Message,
                    });
                }
            }

            return new DynamicSmsResult
            {
                Status = successCount > 0 ? "SUCCESS" : "FAILED",
                Message = resultMessage,
                Items = items
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending dynamic SMS");

            // Fill items for all messages with error
            for (int i = 0; i < messageList.Count; i++)
            {
                items.Add(new DynamicSmsItem
                {
                    SmsStatus = "ERROR",
                    StatusMessage = ex.Message,
                    PhoneNumber = messageList[i].PhoneNumber,
                    CsmsId = $"{batchId}_{i:D4}",
                    SmsBody = messageList[i].Message
                });
            }

            return new DynamicSmsResult
            {
                Status = "FAILED",
                Message = $"Error sending dynamic SMS: {ex.Message}",
                Items = items
            };
        }
    }
}
