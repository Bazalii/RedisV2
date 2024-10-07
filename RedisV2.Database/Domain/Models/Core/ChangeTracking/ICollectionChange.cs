namespace RedisV2.Database.Domain.Models.Core.ChangeTracking;

public interface ICollectionChange
{
    public DateTimeOffset ChangeDate { get; }
}