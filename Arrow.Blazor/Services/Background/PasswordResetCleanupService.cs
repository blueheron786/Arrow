using Arrow.Blazor.Configuration;
using Arrow.Blazor.Data;

namespace Arrow.Blazor.Services.Background;

public class PasswordResetCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PasswordResetCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Run every 6 hours

    public PasswordResetCleanupService(
        IServiceProvider serviceProvider,
        ILogger<PasswordResetCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!FeatureToggles.IsEmailEnabled)
        {
            return;
        }

        _logger.LogInformation("Password reset cleanup service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IPasswordResetRepository>();
                
                _logger.LogInformation("Running password reset token cleanup");
                await repository.DeleteExpiredTokensAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset token cleanup");
            }
        }

        _logger.LogInformation("Password reset cleanup service stopping");
    }
}
