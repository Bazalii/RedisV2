using RedisV2.Database.Domain.Models.Core.ChangeTracking;

namespace RedisV2.Database.Domain.Services.ChangeTracking;

public interface IChangeTracker
{
    void AddChangeToQueue(ICollectionChange change);

    ICollectionChange[] GetAllChanges(
        string collectionName,
        DateTimeOffset startDate,
        DateTimeOffset endDate);

    void ClearChanges(
        string collectionName,
        DateTimeOffset beforeDate);
}