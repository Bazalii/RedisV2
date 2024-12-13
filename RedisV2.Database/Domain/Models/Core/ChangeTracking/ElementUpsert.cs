using RedisV2.Database.Domain.Models.Core.Storage;

namespace RedisV2.Database.Domain.Models.Core.ChangeTracking;

public record ElementUpsert : ICollectionChange
{
    public string Key { get; init; }
    public CollectionElement Element { get; init; }
    public DateTimeOffset ChangeTime { get; init; }
}