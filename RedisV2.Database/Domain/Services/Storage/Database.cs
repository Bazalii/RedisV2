using System.Collections.Concurrent;
using OneOf;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using Timer = System.Timers.Timer;
using static RedisV2.Database.Helpers.OperationResults;

namespace RedisV2.Database.Domain.Services.Storage;

public class Database : IDatabase, IDisposable
{
    private const int CleanupTimeoutInSeconds = 10;
    private Timer? _cleanupTimer;

    private ConcurrentDictionary<string, IDatabaseCollection> _database = new();

    public OneOf<SuccessResult, AlreadyExistsError, UnexpectedError> AddCollection(string collectionName)
    {
        try
        {
            var collection = new DatabaseCollection();

            var isCollectionAdded = _database.TryAdd(collectionName, collection);
            if (isCollectionAdded)
            {
                return new SuccessResult();
            }

            return new AlreadyExistsError($"Collection {collectionName} is already added");
        }
        catch (Exception exception)
        {
            return new UnexpectedError(exception.Message);
        }
    }

    public OneOf<IDatabaseCollection, NotFoundError, UnexpectedError> GetCollection(string collectionName)
    {
        try
        {
            var collection = _database.GetValueOrDefault(collectionName);

            return collection is not null
                ? OneOf<IDatabaseCollection, NotFoundError, UnexpectedError>.FromT0(collection)
                : new NotFoundError("Collection not found");
        }
        catch (Exception exception)
        {
            return new UnexpectedError(exception.Message);
        }
    }

    public OneOf<SuccessResult, UnexpectedError> DeleteCollection(string collectionName) =>
        WithOperationStatus(
            () => _database.TryRemove(collectionName, out _));

    public OneOf<SuccessResult, NotFoundError, UnexpectedError> FlushCollection(string collectionName)
    {
        var getCollectionResult = GetCollection(collectionName);

        return getCollectionResult.Match(
            databaseCollection =>
            {
                var flushCollectionResult = WithOperationStatus(
                    () => databaseCollection.Flush());

                return flushCollectionResult.Match<OneOf<SuccessResult, NotFoundError, UnexpectedError>>(
                    success => success,
                    unexpectedError => unexpectedError);
            },
            notFoundError => notFoundError,
            unexpectedError => unexpectedError);
    }

    public OneOf<SuccessResult, UnexpectedError> Flush() =>
        WithOperationStatus(
            () => _database = new ConcurrentDictionary<string, IDatabaseCollection>());

    public void StartCleanupTimer()
    {
        var cleanupInterval = TimeSpan.FromSeconds(CleanupTimeoutInSeconds);
        _cleanupTimer = new Timer(cleanupInterval);

        _cleanupTimer.Elapsed += (_, _) => CleanExpiredElements();

        _cleanupTimer.AutoReset = true;
        _cleanupTimer.Enabled = true;
    }

    private void CleanExpiredElements()
    {
        foreach (var collection in _database.Values)
        {
            foreach (var (key, element) in collection.GetAll())
            {
                if (element.ExpirationTime < DateTime.UtcNow)
                {
                    collection.Delete(key);
                }
            }
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}