namespace RedisV2.Database.Domain.Models.Core.Storage;

public struct CollectionElement
{
    public string Element { get; init; }
    public DateTime? ExpirationTime { get; init; }
}