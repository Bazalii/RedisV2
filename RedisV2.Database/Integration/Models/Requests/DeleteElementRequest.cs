namespace RedisV2.Database.Integration.Models.Requests;

public record DeleteElementRequest
{
    public string CollectionName { get; init; }
    public string Key { get; init; }
}