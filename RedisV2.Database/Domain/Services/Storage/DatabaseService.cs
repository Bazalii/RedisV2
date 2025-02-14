using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using RedisV2.Database.Helpers;
using OneOf;
using RedisV2.Database.Domain.Factories;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.Core.Storage;
using RedisV2.Database.Domain.Services.ChangeTracking;
using RedisV2.Database.Domain.Services.Registration;
using RedisV2.Database.Domain.Services.Replication;

namespace RedisV2.Database.Domain.Services.Storage;

public class DatabaseService(
    IDatabase database,
    IDiscoveryService discoveryService,
    IReplicasManager replicasManager,
    IChangeTracker changeTracker,
    IDatabaseChangesFactory databaseChangesFactory) : IDatabaseService
{
    public async Task InitAsync(CancellationToken cancellation)
    {
        await changeTracker.LoadAllChangesAsync(cancellation);

        var changes = changeTracker.GetAllStoredChangesAsync();
        foreach (var change in changes)
        {
            ApplyChangeToLocalStorage(change);
        }
    }

    public async Task<OneOf<SuccessResult, AlreadyExistsError, UnexpectedError>> AddCollection(string collectionName)
    {
        var addCollectionResult = database.AddCollection(collectionName);
        if (addCollectionResult.IsNotSuccess())
        {
            return addCollectionResult;
        }

        var nextChangeId = changeTracker.GetNextChangeId();
        var change = databaseChangesFactory.CreateCollectionCreationChange(collectionName, nextChangeId);

        var addChangeToLogResult = await changeTracker.AddChangeToLogAsync(change);
        if (addChangeToLogResult.IsNotSuccess())
        {
            return (UnexpectedError)addChangeToLogResult.Value;
        }

        if (discoveryService.IsNodeLeader())
        {
            await replicasManager.NotifyAllHealthyReplicasAboutChangeAsync(change);
        }

        return new SuccessResult();
    }

    public OneOf<IDatabaseCollection, NotFoundError, UnexpectedError> GetCollection(string collectionName)
    {
        return database.GetCollection(collectionName);
    }

    public async Task<OneOf<SuccessResult, UnexpectedError>> DeleteCollection(string collectionName)
    {
        var collectionDeleteResult = database.DeleteCollection(collectionName);
        if (collectionDeleteResult.IsNotSuccess())
        {
            return collectionDeleteResult;
        }

        var nextChangeId = changeTracker.GetNextChangeId();
        var change = databaseChangesFactory.CreateCollectionDeletionChange(collectionName, nextChangeId);

        var addChangeToLogResult = await changeTracker.AddChangeToLogAsync(change);
        if (addChangeToLogResult.IsNotSuccess())
        {
            return (UnexpectedError)addChangeToLogResult.Value;
        }

        if (discoveryService.IsNodeLeader())
        {
            await replicasManager.NotifyAllHealthyReplicasAboutChangeAsync(change);
        }

        return new SuccessResult();
    }

    public async Task<OneOf<SuccessResult, NotFoundError, UnexpectedError>> FlushCollection(string collectionName)
    {
        var collectionDeleteResult = database.FlushCollection(collectionName);
        if (collectionDeleteResult.IsNotSuccess())
        {
            return collectionDeleteResult;
        }

        var nextChangeId = changeTracker.GetNextChangeId();
        var change = databaseChangesFactory.CreateCollectionFlushChange(collectionName, nextChangeId);

        var addChangeToLogResult = await changeTracker.AddChangeToLogAsync(change);
        if (addChangeToLogResult.IsNotSuccess())
        {
            return (UnexpectedError)addChangeToLogResult.Value;
        }

        if (discoveryService.IsNodeLeader())
        {
            await replicasManager.NotifyAllHealthyReplicasAboutChangeAsync(change);
        }

        return new SuccessResult();
    }

    public Task<OneOf<SuccessResult, UnexpectedError>> Flush()
    {
        throw new NotImplementedException();
    }

    public async Task<OneOf<SuccessResult, NotFoundError, UnexpectedError>> UpsertElement(
        string collectionName,
        string key,
        string value,
        TimeSpan? expiry)
    {
        var operationTime = DateTimeOffset.UtcNow;

        DateTimeOffset? expirationTime = expiry is null
            ? null
            : operationTime.Add(expiry.Value);

        var element = new CollectionElement
        {
            Value = value,
            ExpirationTime = expirationTime
        };

        var upsertElementResult = UpsertElementToLocalStorage(collectionName, key, element);
        if (upsertElementResult.IsNotSuccess())
        {
            return upsertElementResult.Value switch
            {
                NotFoundError notFoundError => notFoundError,
                UnexpectedError unexpectedError => unexpectedError,
            };
        }

        var upsertedElement = (CollectionElement)upsertElementResult.Value;

        var nextChangeId = changeTracker.GetNextChangeId();
        var change = databaseChangesFactory.CreateElementUpsertChange(
            collectionName, key, upsertedElement, nextChangeId);

        var addChangeToLogResult = await changeTracker.AddChangeToLogAsync(change);
        if (addChangeToLogResult.IsNotSuccess())
        {
            return (UnexpectedError)addChangeToLogResult.Value;
        }

        if (discoveryService.IsNodeLeader())
        {
            await replicasManager.NotifyAllHealthyReplicasAboutChangeAsync(change);
        }

        return new SuccessResult();
    }

    private OneOf<CollectionElement, NotFoundError, UnexpectedError> UpsertElementToLocalStorage(
        string collectionName,
        string key,
        CollectionElement element)
    {
        var getCollectionResult = database.GetCollection(collectionName);
        if (getCollectionResult.IsNotSuccess())
        {
            return getCollectionResult.Value switch
            {
                NotFoundError notFoundError => notFoundError,
                UnexpectedError unexpectedError => unexpectedError,
            };
        }

        var collection = (IDatabaseCollection)getCollectionResult.Value;

        var upsertElementResult = collection.Upsert(key, element);
        if (upsertElementResult.IsNotSuccess())
        {
            return (UnexpectedError)upsertElementResult.Value;
        }

        return (CollectionElement)upsertElementResult.Value;
    }

    public OneOf<string, NotFoundError, UnexpectedError> GetElement(
        string collectionName,
        string key)
    {
        var getCollectionResult = database.GetCollection(collectionName);
        if (getCollectionResult.IsNotSuccess())
        {
            return getCollectionResult.Value switch
            {
                NotFoundError notFoundError => notFoundError,
                UnexpectedError unexpectedError => unexpectedError,
            };
        }

        var collection = (IDatabaseCollection)getCollectionResult.Value;
        var getElementResult = collection.Get(key);
        if (getElementResult.IsNotSuccess())
        {
            return (NotFoundError)getElementResult.Value;
        }

        return (string)getElementResult.Value;
    }

    public async Task<OneOf<SuccessResult, NotFoundError, UnexpectedError>> DeleteElement(
        string collectionName,
        string key)
    {
        var deleteElementResult = DeleteElementFromLocalStorage(collectionName, key);
        if (deleteElementResult.IsNotSuccess())
        {
            return deleteElementResult;
        }

        var nextChangeId = changeTracker.GetNextChangeId();
        var change = databaseChangesFactory.CreateElementDeletionChange(
            collectionName, key, nextChangeId);

        var addChangeToLogResult = await changeTracker.AddChangeToLogAsync(change);
        if (addChangeToLogResult.IsNotSuccess())
        {
            return (UnexpectedError)addChangeToLogResult.Value;
        }

        if (discoveryService.IsNodeLeader())
        {
            await replicasManager.NotifyAllHealthyReplicasAboutChangeAsync(change);
        }

        return new SuccessResult();
    }

    public long GetLastChangeId() => changeTracker.GetLastChangeId();

    public IDatabaseChange GetChangeById(long id)
    {
        throw new NotImplementedException();
    }

    public void StartCleanupTimer() => database.StartCleanupTimer();

    private OneOf<SuccessResult, NotFoundError, UnexpectedError> DeleteElementFromLocalStorage(
        string collectionName,
        string key)
    {
        var getCollectionResult = database.GetCollection(collectionName);
        if (getCollectionResult.IsNotSuccess())
        {
            return getCollectionResult.Value switch
            {
                NotFoundError notFoundError => notFoundError,
                UnexpectedError unexpectedError => unexpectedError,
            };
        }

        var collection = (IDatabaseCollection)getCollectionResult.Value;
        var deleteElementResult = collection.Delete(key);
        if (deleteElementResult.IsNotSuccess())
        {
            return (UnexpectedError)deleteElementResult.Value;
        }

        return new SuccessResult();
    }

    public async Task<OneOf<SuccessResult, UnexpectedError>> ApplyChange(IDatabaseChange change)
    {
        var applyChangeToLocalStorageResult = ApplyChangeToLocalStorage(change);
        if (applyChangeToLocalStorageResult.IsNotSuccess())
        {
            return applyChangeToLocalStorageResult;
        }

        return await changeTracker.AddChangeToLogAsync(change);
    }

    private OneOf<SuccessResult, UnexpectedError> ApplyChangeToLocalStorage(IDatabaseChange change)
    {
        switch (change)
        {
            case CollectionCreation collectionCreation:
                database.AddCollection(collectionCreation.CollectionName);
                break;
            case CollectionDeletion collectionDeletion:
                database.DeleteCollection(collectionDeletion.CollectionName);
                break;
            case CollectionFlush collectionFlush:
                database.FlushCollection(collectionFlush.CollectionName);
                break;
            case ElementUpsert elementUpsert:
                UpsertElementToLocalStorage(
                    elementUpsert.CollectionName,
                    elementUpsert.Key,
                    elementUpsert.Element);
                break;
            case ElementDeletion elementDeletion:
                DeleteElementFromLocalStorage(elementDeletion.CollectionName, elementDeletion.Key);
                break;
        }

        return new SuccessResult();
    }
}