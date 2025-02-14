using System.Text.Json;
using System.Text.Json.Serialization;
using CommonLibrary.Extensions;
using RedisV2.Discovery.Domain.NodeClients;
using RedisV2.Discovery.Domain.Services;

namespace RedisV2.Discovery.Extensions;

public static class DependencyInjectionExtensions
{
    public static void AddDependencies(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddServices()
            .AddHttpClients()
            .AddEndpointsApiExplorer()
            .AddControllers()
            .AddJsonOptions(configurator =>
            {
                configurator.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                configurator.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddDefaultWebClientPolicy();

        serviceCollection
            .AddHttpClient<LeaderServiceClient>()
            .AddPolicyHandlerFromRegistry("default");
        serviceCollection
            .AddHttpClient<ReplicaServiceClient>()
            .AddPolicyHandlerFromRegistry("default");

        return serviceCollection;
    }

    private static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ISystemStateService, SystemStateService>();

        return serviceCollection;
    }
}