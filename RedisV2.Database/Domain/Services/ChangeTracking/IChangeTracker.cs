using RedisV2.Database.Domain.Models.Core.ChangeTracking;

namespace RedisV2.Database.Domain.Services.ChangeTracking;

public interface IChangeTracker
{
    void AddChangeToQueue(ICollectionChange change);

    ICollectionChange[] GetAllChanges(DateTimeOffset startTime);

    void ClearChanges(DateTimeOffset beforeTime);
}