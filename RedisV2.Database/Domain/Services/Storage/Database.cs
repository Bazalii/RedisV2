using System.Collections.Concurrent;

namespace RedisV2.Database.Domain.Services.Storage;

public class Database : IDatabase
{
    private ConcurrentDictionary<string, IDatabaseCollection> _database = new();

    public void AddCollection(string collectionName)
    {
        var collection = new DatabaseCollection();
        _database.TryAdd(collectionName, collection);
    }

    public IDatabaseCollection? GetCollection(string collectionName) => _database.GetValueOrDefault(collectionName);

    public void DeleteCollection(string collectionName) => _database.TryRemove(collectionName, out _);

    public void FlushCollection(string collectionName)
    {
        var collection = GetCollection(collectionName);

        collection?.Flush();
    }

    public void Flush() => _database = new ConcurrentDictionary<string, IDatabaseCollection>();
}