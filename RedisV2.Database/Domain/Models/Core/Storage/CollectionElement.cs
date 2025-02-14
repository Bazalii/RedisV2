namespace RedisV2.Database.Domain.Models.Core.Storage;

public struct CollectionElement
{
    public string Value { get; init; }
    public DateTimeOffset? ExpirationTime { get; init; }
}