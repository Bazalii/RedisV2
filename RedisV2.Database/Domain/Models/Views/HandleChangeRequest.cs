using RedisV2.Database.Domain.Models.Core.ChangeTracking;

namespace RedisV2.Database.Domain.Models.Views;

public record HandleChangeRequest
{
    public required IDatabaseChange Change { get; init; }
}