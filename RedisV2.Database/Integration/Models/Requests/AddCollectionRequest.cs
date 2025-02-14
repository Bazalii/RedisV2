namespace RedisV2.Database.Integration.Models.Requests;

public record AddCollectionRequest
{
    public required string Name { get; init; }
}