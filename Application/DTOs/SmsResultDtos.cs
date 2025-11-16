namespace SmsEngine.Application.DTOs;

public record SmsResult(bool Success, string? Message = null, string? ReferenceId = null);
public record BulkSmsResult(bool Success, string? ReferenceId = null, string? Message = null);
public record DynamicSmsResult(bool Success, Dictionary<string, string>? ReferenceIds = null, string? Message = null);
