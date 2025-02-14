using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.Core.Storage;

namespace RedisV2.Database.Domain.Factories;

public class DatabaseChangesFactory : IDatabaseChangesFactory
{
    public CollectionCreation CreateCollectionCreationChange(
        string collectionName,
        long changeId) =>
        new()
        {
            CollectionName = collectionName,
            ChangeTime = DateTimeOffset.UtcNow,
            Id = changeId,
        };

    public CollectionDeletion CreateCollectionDeletionChange(
        string collectionName,
        long changeId) =>
        new()
        {
            CollectionName = collectionName,
            ChangeTime = DateTimeOffset.UtcNow,
            Id = changeId,
        };

    public CollectionFlush CreateCollectionFlushChange(
        string collectionName,
        long changeId) =>
        new()
        {
            CollectionName = collectionName,
            ChangeTime = DateTimeOffset.UtcNow,
            Id = changeId,
        };

    public ElementUpsert CreateElementUpsertChange(
        string collectionName,
        string key,
        CollectionElement value,
        long changeId) =>
        new()
        {
            CollectionName = collectionName,
            Key = key,
            Element = value,
            ChangeTime = DateTimeOffset.UtcNow,
            Id = changeId,
        };

    public ElementDeletion CreateElementDeletionChange(
        string collectionName,
        string key,
        long changeId) =>
        new()
        {
            CollectionName = collectionName,
            Key = key,
            ChangeTime = DateTimeOffset.UtcNow,
            Id = changeId,
        };
}