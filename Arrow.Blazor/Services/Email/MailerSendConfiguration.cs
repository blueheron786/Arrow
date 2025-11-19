namespace Arrow.Blazor.Services.Email;

public class MailerSendConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Arrow";
    public string BaseUrl { get; set; } = "https://arrow.local";
}
