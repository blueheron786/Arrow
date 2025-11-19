using System;
using System.Text;
using Arrow.Blazor.Contracts.Email;

namespace Arrow.Blazor.Services.Email;

public class EmailTemplateService
{
    private readonly string _baseUrl;

    public EmailTemplateService(string baseUrl)
    {
        _baseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? "https://arrow.local"
            : baseUrl.TrimEnd('/');
    }

    public EmailTemplate CreateWelcomeTemplate(string email, string unsubscribeToken)
    {
        var unsubscribeUrl = $"{_baseUrl}/unsubscribe?token={Uri.EscapeDataString(unsubscribeToken)}";

        var htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<h1>Welcome to Arrow!</h1>");
        htmlBuilder.AppendLine("<p>We'll keep you posted whenever something interesting happens.</p>");
        htmlBuilder.AppendLine($"<p><a href=\"{unsubscribeUrl}\">Unsubscribe</a> at any time.</p>");

        var textBuilder = new StringBuilder();
        textBuilder.AppendLine("Welcome to Arrow!");
        textBuilder.AppendLine("We'll keep you posted whenever something interesting happens.");
        textBuilder.AppendLine($"Unsubscribe: {unsubscribeUrl}");

        return new EmailTemplate
        {
            To = email,
            Subject = "Welcome to Arrow",
            HtmlContent = htmlBuilder.ToString(),
            TextContent = textBuilder.ToString()
        };
    }
}
