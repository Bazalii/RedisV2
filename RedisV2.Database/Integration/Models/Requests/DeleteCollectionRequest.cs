namespace RedisV2.Database.Integration.Models.Requests;

public record DeleteCollectionRequest
{
    public required string Name { get; init; }
}