using RedisV2.Database.Domain.Models.Core.ChangeTracking;

namespace RedisV2.Database.Domain.Services.Storage;

public interface IDatabaseCollection
{
    void Upsert(
        string key,
        string value,
        TimeSpan? expiry);

    string Get(string key);
    void Delete(string key);

    void Flush();

    void ApplyChanges(ICollectionChange[] changes);
}