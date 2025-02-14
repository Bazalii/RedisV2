namespace RedisV2.Database.Integration.Models.Requests;

public record UpsertElementRequest
{
    public required string CollectionName { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
    public TimeSpan? Expiry { get; init; }
}