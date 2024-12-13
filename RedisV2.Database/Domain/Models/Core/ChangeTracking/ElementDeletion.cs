namespace RedisV2.Database.Domain.Models.Core.ChangeTracking;

public record ElementDeletion : ICollectionChange
{
    public string Key { get; init; }
    public DateTimeOffset ChangeTime { get; init; }
}