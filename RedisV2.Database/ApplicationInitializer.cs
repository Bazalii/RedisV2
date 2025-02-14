using RedisV2.Database.Domain.Services.Registration;
using RedisV2.Database.Domain.Services.Storage;

namespace RedisV2.Database;

public class ApplicationInitializer(
    IDatabaseService databaseService,
    IDiscoveryService discoveryService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellation)
    {
        await databaseService.InitAsync(cancellation);

        var lastChangeId = databaseService.GetLastChangeId();

        await discoveryService.RegisterAsync(lastChangeId);

        if (discoveryService.IsNodeLeader())
        {
            databaseService.StartCleanupTimer();
        }
    }

    public Task StopAsync(CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }
}