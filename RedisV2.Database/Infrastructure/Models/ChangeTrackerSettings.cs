namespace RedisV2.Database.Infrastructure.Models;

public record ChangeTrackerSettings
{
    public required string ChangesFileName { get; init; }
    public required string LastSavedChangeIdFileName { get; init; }
}