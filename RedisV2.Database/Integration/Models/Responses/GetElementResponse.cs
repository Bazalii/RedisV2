namespace RedisV2.Database.Integration.Models.Responses;

public record GetElementResponse
{
    public required string Value { get; init; }
}