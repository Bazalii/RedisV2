using System.Text.Json;
using System.Text.Json.Serialization;
using CommonLibrary.Extensions;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using RedisV2.Database.Domain.Factories;
using RedisV2.Database.Domain.ServiceClients;
using RedisV2.Database.Domain.Services.ChangeTracking;
using RedisV2.Database.Domain.Services.Registration;
using RedisV2.Database.Domain.Services.Replication;
using RedisV2.Database.Domain.Services.Storage;
using RedisV2.Database.Infrastructure.Models;

namespace RedisV2.Database.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var serviceCollection = builder.Services;

        serviceCollection.AddHostedService<ApplicationInitializer>();

        serviceCollection.AddHealthChecks();

        serviceCollection
            .AddHttpClients()
            .AddServices()
            .AddFactories()
            .AddSettings(configuration)
            .AddControllers()
            .AddJsonOptions(configurator =>
            {
                configurator.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                configurator.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
    }

    private static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDatabase, Domain.Services.Storage.Database>();
        serviceCollection.AddSingleton<IChangeTracker, ChangeTracker>();
        serviceCollection.AddSingleton<IDatabaseService, DatabaseService>();
        serviceCollection.AddSingleton<IDiscoveryService, DiscoveryService>();
        serviceCollection.AddSingleton<IReplicasManager, ReplicasManager>();

        return serviceCollection;
    }

    private static IServiceCollection AddFactories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDatabaseChangesFactory, DatabaseChangesFactory>();

        return serviceCollection;
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddDefaultWebClientPolicy();

        serviceCollection
            .AddHttpClient<DiscoveryServiceClient>()
            .AddPolicyHandlerFromRegistry("default");

        serviceCollection
            .AddHttpClient<ReplicaServiceClient>()
            .AddPolicyHandlerFromRegistry("default");

        return serviceCollection;
    }

    private static IServiceCollection AddSettings(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.Configure<ChangeTrackerSettings>(
            configuration.GetSection(nameof(ChangeTrackerSettings)));
        serviceCollection.Configure<ServiceDiscoverySettings>(
            configuration.GetSection(nameof(ServiceDiscoverySettings)));
        serviceCollection.Configure<ServiceSettings>(
            configuration.GetSection(nameof(ServiceSettings)));

        return serviceCollection;
    }
}