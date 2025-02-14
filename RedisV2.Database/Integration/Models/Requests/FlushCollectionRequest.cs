namespace RedisV2.Database.Integration.Models.Requests;

public record FlushCollectionRequest
{
    public string Name { get; init; }
}