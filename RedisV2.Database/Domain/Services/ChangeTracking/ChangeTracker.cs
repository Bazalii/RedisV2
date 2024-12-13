using System.Collections.Concurrent;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;

namespace RedisV2.Database.Domain.Services.ChangeTracking;

public class ChangeTracker : IChangeTracker
{
    private readonly BlockingCollection<ICollectionChange> _changesQueue = new(
        new ConcurrentQueue<ICollectionChange>());

    private readonly object _lock = new();
    private readonly LinkedList<ICollectionChange> _changes = [];

    public ChangeTracker()
    {
        StartCollectionChangesProcessing();
    }

    private void StartCollectionChangesProcessing()
    {
        Task.Factory.StartNew(ProcessCollectionChanges, TaskCreationOptions.LongRunning);
    }

    private void ProcessCollectionChanges()
    {
        foreach (var collectionChange in _changesQueue.GetConsumingEnumerable())
        {
            lock (_lock)
            {
                _changes.AddLast(collectionChange);
            }
        }
    }

    public void AddChangeToQueue(ICollectionChange change)
    {
        _changesQueue.Add(change);
    }

    public ICollectionChange[] GetAllChanges(DateTimeOffset startTime)
    {
        lock (_lock)
        {
            return _changes
                .Where(change => change.ChangeTime >= startTime)
                .ToArray();
        }
    }

    public void ClearChanges(DateTimeOffset beforeTime)
    {
        lock (_lock)
        {
            while (_changes.First?.Value.ChangeTime <= beforeTime)
            {
                _changes.RemoveFirst();
            }
        }
    }
}