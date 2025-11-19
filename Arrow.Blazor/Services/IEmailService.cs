namespace Arrow.Blazor.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string unsubscribeToken);
    Task SendGenericEmailAsync(string to, string subject, string body);
    Task<bool> IsHealthyAsync();
}
