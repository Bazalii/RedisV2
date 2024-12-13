using System.Collections.Concurrent;
using OneOf;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.Core.Storage;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using RedisV2.Database.Domain.Services.ChangeTracking;
using static RedisV2.Database.Helpers.OperationResults;
using Timer = System.Timers.Timer;

namespace RedisV2.Database.Domain.Services.Storage;

public class DatabaseCollection : IDatabaseCollection, IDisposable
{
    private const int CleanupTimeoutInSeconds = 10;

    private readonly IChangeTracker _changeTracker;

    private ConcurrentDictionary<string, CollectionElement> _elements = new();
    private readonly Timer _cleanupTimer;

    public DatabaseCollection(IChangeTracker changeTracker)
    {
        _changeTracker = changeTracker;

        var cleanupInterval = TimeSpan.FromSeconds(CleanupTimeoutInSeconds);
        _cleanupTimer = new Timer(cleanupInterval);

        _cleanupTimer.Elapsed += (_, _) => CleanExpiredElements();

        _cleanupTimer.AutoReset = true;
        _cleanupTimer.Enabled = true;
    }

    public OneOf<SuccessResult, UnexpectedError> Upsert(
        string key,
        string value,
        TimeSpan? expiry) =>
        WithOperationStatus(() =>
        {
            var operationTime = DateTimeOffset.UtcNow;

            DateTimeOffset? expirationTime = expiry is null
                ? null
                : operationTime.Add(expiry.Value);

            var element = new CollectionElement
            {
                Element = value,
                ExpirationTime = expirationTime
            };

            Upsert(key, element);

            TrackUpsertChange(key, element, operationTime);
        });

    private void Upsert(string key, CollectionElement element) => _elements[key] = element;

    public OneOf<string, NotFoundError> Get(string key) =>
        _elements.TryGetValue(key, out var element)
            ? element.Element
            : new NotFoundError("Key not found");

    public OneOf<SuccessResult, UnexpectedError> Delete(string key) =>
        WithOperationStatus(
            () =>
            {
                var operationTime = DateTimeOffset.UtcNow;

                _elements.Remove(key, out _);

                TrackDeletionChange(key, operationTime);
            });

    public OneOf<SuccessResult, UnexpectedError> Flush() =>
        WithOperationStatus(
            () => _elements = new ConcurrentDictionary<string, CollectionElement>());

    public OneOf<SuccessResult, UnexpectedError> ApplyChanges(ICollectionChange[] changes) =>
        WithOperationStatus(
            () =>
            {
                foreach (var change in changes)
                {
                    switch (change)
                    {
                        case ElementDeletion deletion:
                            Delete(deletion.Key);
                            break;
                        case ElementUpsert upsert:
                            Upsert(upsert.Key, upsert.Element);
                            break;
                    }
                }
            });

    private void CleanExpiredElements()
    {
        foreach (var (key, element) in _elements)
        {
            if (element.ExpirationTime < DateTime.UtcNow)
            {
                _elements.TryRemove(key, out _);
            }
        }
    }

    private void TrackUpsertChange(
        string key,
        CollectionElement element,
        DateTimeOffset changeTime)
    {
        var upsertChange = new ElementUpsert
        {
            Key = key,
            Element = element,
            ChangeTime = changeTime
        };

        _changeTracker.AddChangeToQueue(upsertChange);
    }

    private void TrackDeletionChange(
        string key,
        DateTimeOffset changeTime)
    {
        var deletionChange = new ElementDeletion
        {
            Key = key,
            ChangeTime = changeTime
        };

        _changeTracker.AddChangeToQueue(deletionChange);
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
    }
}