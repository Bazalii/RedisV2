using System.Collections.Concurrent;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;

namespace RedisV2.Database.Domain.Services.ChangeTracking;

public class ChangeTracker : IChangeTracker
{
    private readonly object _lock = new();

    private readonly BlockingCollection<ICollectionChange> _changesQueue = new(
        new ConcurrentQueue<ICollectionChange>());

    private readonly LinkedList<ICollectionChange> _changes = [];

    public ChangeTracker()
    {
    }

    public void AddChangeToQueue(ICollectionChange change)
    {
        _changesQueue.Add(change);
    }

    public ICollectionChange[] GetAllChanges(
        string collectionName,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        lock (_lock)
        {
            return _changes
                .Where(change => change.ChangeDate >= startDate && change.ChangeDate <= endDate)
                .ToArray();
        }
    }

    public void ClearChanges(
        string collectionName,
        DateTimeOffset beforeDate)
    {
        throw new NotImplementedException();
    }
}