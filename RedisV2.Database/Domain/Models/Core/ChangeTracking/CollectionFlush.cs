namespace RedisV2.Database.Domain.Models.Core.ChangeTracking;

public record CollectionFlush : IDatabaseChange
{
    public required string CollectionName { get; init; }
    public required DateTimeOffset ChangeTime { get; init; }
    public required long Id { get; init; }
}