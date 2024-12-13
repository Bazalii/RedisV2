namespace RedisV2.Database.Domain.Models.Core.ChangeTracking;

public interface ICollectionChange
{
    DateTimeOffset ChangeTime { get; }
}