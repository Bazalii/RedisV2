using System.Collections.Concurrent;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.Core.Storage;
using Timer = System.Timers.Timer;

namespace RedisV2.Database.Domain.Services.Storage;

public class DatabaseCollection : IDatabaseCollection, IDisposable
{
    private const int CleanupTimeoutInSeconds = 10;

    private ConcurrentDictionary<string, CollectionElement> _elements = new();
    private readonly Timer _cleanupTimer;

    public DatabaseCollection()
    {
        var cleanupInterval = TimeSpan.FromSeconds(CleanupTimeoutInSeconds);
        _cleanupTimer = new Timer(cleanupInterval);

        _cleanupTimer.Elapsed += (_, _) => CleanExpiredElements();

        _cleanupTimer.AutoReset = true;
        _cleanupTimer.Enabled = true;
    }

    public void Upsert(string key, string value, TimeSpan? expiry)
    {
        DateTime? expirationTime = expiry is null
            ? null
            : DateTime.UtcNow.Add(expiry.Value);

        var element = new CollectionElement
        {
            Element = value,
            ExpirationTime = expirationTime
        };

        _elements[key] = element;
    }

    public string Get(string key) => _elements[key].Element;

    public void Delete(string key) => _elements.TryRemove(key, out _);

    public void Flush() => _elements = new ConcurrentDictionary<string, CollectionElement>();

    public void ApplyChanges(ICollectionChange[] changes)
    {
        throw new NotImplementedException();
    }

    private void CleanExpiredElements()
    {
        foreach (var (key, element) in _elements)
        {
            if (element.ExpirationTime > DateTime.UtcNow)
            {
                _elements.Remove(key, out _);
            }
        }
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
    }
}