namespace RedisV2.Database.Infrastructure.Models;

public record ServiceSettings
{
    public required string Name { get; init; }
    public required int Port { get; init; }
}