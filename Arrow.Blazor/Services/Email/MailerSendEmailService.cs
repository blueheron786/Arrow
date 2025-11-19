using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Arrow.Blazor.Contracts.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arrow.Blazor.Services.Email;

public class MailerSendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly MailerSendConfiguration _config;
    private readonly EmailTemplateService _templateService;
    private readonly ILogger<MailerSendEmailService> _logger;

    public MailerSendEmailService(
        IHttpClientFactory httpClientFactory,
        IOptions<MailerSendConfiguration> config,
        ILogger<MailerSendEmailService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _config = config.Value;
        _templateService = new EmailTemplateService(_config.BaseUrl);
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string unsubscribeToken)
    {
        try
        {
            var template = _templateService.CreateWelcomeTemplate(email, unsubscribeToken);
            await SendEmailAsync(template);
            _logger.LogInformation("Welcome email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            throw;
        }
    }

    public async Task SendGenericEmailAsync(string to, string subject, string body)
    {
        var template = new EmailTemplate
        {
            To = to,
            Subject = subject,
            HtmlContent = $"<p>{body}</p>",
            TextContent = body
        };

        await SendEmailAsync(template);
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.mailersend.com/v1/domains");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MailerSend health check failed");
            return false;
        }
    }

    private async Task SendEmailAsync(EmailTemplate template)
    {
        var payload = new
        {
            from = new { email = _config.FromEmail, name = _config.FromName },
            to = new[] { new { email = template.To } },
            subject = template.Subject,
            html = template.HtmlContent,
            text = template.TextContent
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mailersend.com/v1/email")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"MailerSend error: {response.StatusCode} — {body}");
        }
    }
}
