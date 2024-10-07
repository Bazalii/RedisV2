using RedisV2.Database.Domain.Models.Core.Storage;

namespace RedisV2.Database.Domain.Models.Core.ChangeTracking;

public record ElementUpsert : ICollectionChange
{
    public CollectionElement Element { get; init; }
    public DateTimeOffset ChangeDate { get; init; }
}