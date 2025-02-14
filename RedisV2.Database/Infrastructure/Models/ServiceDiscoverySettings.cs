namespace RedisV2.Database.Infrastructure.Models;

public record ServiceDiscoverySettings
{
    public required string ServiceAddress { get; init; }
    // public required string NodeIdFileName { get; init; }
}