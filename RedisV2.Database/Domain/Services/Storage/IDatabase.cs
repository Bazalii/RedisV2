namespace RedisV2.Database.Domain.Services.Storage;

public interface IDatabase
{
    void AddCollection(string collectionName);
    IDatabaseCollection? GetCollection(string collectionName);
    void DeleteCollection(string collectionName);
    void FlushCollection(string collectionName);
    void Flush();
}