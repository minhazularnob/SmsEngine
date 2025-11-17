// SmsResultDtos.cs
using System.Collections.Generic;

namespace SmsEngine.Application.DTOs
{
    /// <summary>
    /// Single SMS response
    /// </summary>
    public class SingleSmsResult
    {
        public string? Status { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ReferenceId { get; set; }
        public string? StatusCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CsmsId { get; set; }
        public string? SmsType { get; set; }
        public string? SmsBody { get; set; }
    }

    /// <summary>
    /// Bulk SMS response
    /// </summary>
    public class BulkSmsResult
    {
        public string? Status { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? BatchId { get; set; }
        public List<BulkSmsItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Bulk SMS item response
    /// </summary>
    public class BulkSmsItem
    {
        public string? ReferenceId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CsmsId { get; set; }
        public string? SmsType { get; set; }
        public string? SmsBody { get; set; }
    }

    /// <summary>
    /// Dynamic SMS response
    /// </summary>
    public class DynamicSmsResult
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public List<DynamicSmsItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Dynamic SMS item response
    /// </summary>
    public class DynamicSmsItem
    {
        public string? ReferenceId { get; set; }
        public string? SmsStatus { get; set; }
        public string? StatusMessage { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CsmsId { get; set; }
        public string? SmsType { get; set; }
        public string? SmsBody { get; set; }
    }
}