using Aiursoft.Kahla.Server.Data;

namespace Aiursoft.Kahla.Server.Services.BackgroundJobs;

public class UnreadPersistsService(
    ILogger<UnreadPersistsService> logger,
    IServiceScopeFactory scopeFactory)
    : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly ILogger _logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UnreadPersistsService service is starting...");
        // Delay start for 30 seconds, then run every 2 minutes.
        _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2));
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        DoWorkAsync().GetAwaiter().GetResult();
    }

    public async Task DoWorkAsync()
    {
        _logger.LogInformation("UnreadPersistsService is working.");
        using var scope = scopeFactory.CreateScope();
        var memoryAccess = scope.ServiceProvider.GetRequiredService<QuickMessageAccess>();
        await memoryAccess.PersistUserUnreadAmount();
        _logger.LogInformation("UnreadPersistsService is done.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email notifier service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}