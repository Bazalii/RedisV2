using RedisV2.Database.Domain.Services.Storage;

namespace RedisV2.Database.Extensions;

public static class DependencyInjectionExtensions
{
    public static void AddDependencies(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDatabase, Domain.Services.Storage.Database>();

        serviceCollection.AddGrpc();
    }
}