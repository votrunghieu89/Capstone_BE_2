using Capstone_2_BE.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Capstone_2_BE.Settings;

public sealed class NotificationCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationCleanupBackgroundService> _logger;

    // Run cleanup every hour (adjust as needed)
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public NotificationCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Optional: run once at startup
        await RunOnce(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnce(stoppingToken);
        }
    }

    private async Task RunOnce(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepo>();

            var ok = await repo.DeleteNotification();
            if (!ok)
            {
                _logger.LogWarning("Notification cleanup returned false (DeleteNotification)");
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running notification cleanup background job");
        }
    }
}
