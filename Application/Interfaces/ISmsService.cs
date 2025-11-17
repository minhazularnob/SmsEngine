// ISmsService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsEngine.Application.DTOs;
using SmsEngine.Application.DTOs.Requests;

namespace SmsEngine.Application.Interfaces
{
    /// <summary>
    /// Interface for sending SMS messages
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Sends a single SMS message
        /// </summary>
        /// <param name="message">The SMS message to send</param>
        /// <returns>SendSingleSmsAsync with the status of the operation</returns>
        Task<SingleSmsResult> SendSingleSmsAsync(SingleSmsMessageRequest message);

        /// <summary>
        /// Sends the same SMS message to multiple recipients
        /// </summary>
        /// <param name="messages">Bulk SMS request containing recipients and message</param>
        /// <returns>BulkSmsResult with status for each recipient</returns>
        Task<BulkSmsResult> SendBulkSmsAsync(BulkSmsRequest messages);

        /// <summary>
        /// Sends dynamic SMS messages with different content to each recipient
        /// </summary>
        /// <param name="request">Dynamic SMS request containing multiple messages</param>
        /// <returns>DynamicSmsResult with status for each message</returns>
        Task<DynamicSmsResult> SendDynamicSmsAsync(DynamicSmsRequest request);
    }
}