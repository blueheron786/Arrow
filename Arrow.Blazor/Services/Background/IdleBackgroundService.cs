using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arrow.Blazor.Services.Background;

/// <summary>
/// A sample background service that runs daily.
/// </summary>
public class IdleBackgroundService : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromDays(1);
    private readonly ILogger<IdleBackgroundService> _logger;

    public IdleBackgroundService(ILogger<IdleBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Idle background service starting");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Idle background service tick");
                await Task.Delay(TickInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // expected when host is shutting down
        }
        finally
        {
            _logger.LogInformation("Idle background service stopping");
        }
    }
}
