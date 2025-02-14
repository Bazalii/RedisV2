using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.Core.Storage;

namespace RedisV2.Database.Domain.Factories;

public interface IDatabaseChangesFactory
{
    CollectionCreation CreateCollectionCreationChange(
        string collectionName,
        long changeId);

    CollectionDeletion CreateCollectionDeletionChange(
        string collectionName,
        long changeId);

    CollectionFlush CreateCollectionFlushChange(
        string collectionName,
        long changeId);

    ElementUpsert CreateElementUpsertChange(
        string collectionName,
        string key,
        CollectionElement value,
        long changeId);

    ElementDeletion CreateElementDeletionChange(
        string collectionName,
        string key,
        long changeId);
}