using System.Collections.Concurrent;
using OneOf;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using RedisV2.Database.Domain.Services.ChangeTracking;
using static RedisV2.Database.Helpers.OperationResults;

namespace RedisV2.Database.Domain.Services.Storage;

public class Database : IDatabase
{
    private ConcurrentDictionary<string, IDatabaseCollection> _database = new();

    public OneOf<SuccessResult, AlreadyExistsError, UnexpectedError> AddCollection(string collectionName)
    {
        try
        {
            var changeTracker = new ChangeTracker();
            var collection = new DatabaseCollection(changeTracker);

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
}