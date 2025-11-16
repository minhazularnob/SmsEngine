namespace SmsEngine.Infrastructure.Configuration;

public class SmsSettings
{
    public const string SectionName = "SmsSettings";
    
    public string ApiToken { get; set; } = string.Empty;
    public string Sid { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}
