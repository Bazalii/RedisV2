using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using OneOf;
using System.Text.Json;
using CommonLibrary.Serialization;
using Microsoft.Extensions.Options;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using RedisV2.Database.Infrastructure.Models;

namespace RedisV2.Database.Domain.Services.ChangeTracking;

public class ChangeTracker(
    IOptions<ChangeTrackerSettings> settings)
    : IChangeTracker
{
    private readonly string _changesFileName = settings.Value.ChangesFileName;
    private readonly string _lastSavedChangeIdFileName = settings.Value.LastSavedChangeIdFileName;

    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly ConcurrentQueue<IDatabaseChange> _changes = [];
    private long _lastSavedChangeId;

    public async Task LoadAllChangesAsync(CancellationToken cancellation)
    {
        var maxChangeId = 0L;
        await foreach (var change in GetAllChangesAsync(cancellation))
        {
            _changes.Enqueue(change);
            if (change.Id > maxChangeId)
            {
                maxChangeId = change.Id;
            }
        }

        _lastSavedChangeId = maxChangeId;
    }

    public async Task<OneOf<SuccessResult, UnexpectedError>> AddChangeToLogAsync(IDatabaseChange databaseChange)
    {
        _changes.Enqueue(databaseChange);

        var serializedChange = JsonSerializer.Serialize(databaseChange, JsonSerializationOptions.Default);

        await _lock.WaitAsync();
        try
        {
            await using var changesStream = File.AppendText(_changesFileName);
            await changesStream.WriteLineAsync(serializedChange);
            await File.WriteAllTextAsync(_lastSavedChangeIdFileName, databaseChange.Id.ToString());
        }
        catch (Exception exception)
        {
            return new UnexpectedError(exception.Message);
        }
        finally
        {
            _lock.Release();
        }

        return new SuccessResult();
    }

    public long GetLastChangeId()
    {
        return Interlocked.Read(ref _lastSavedChangeId);
    }

    public long GetNextChangeId()
    {
        return Interlocked.Increment(ref _lastSavedChangeId);
    }

    public IDatabaseChange GetChangeById(long id) => _changes.First(x => x.Id == id);

    public IReadOnlyCollection<IDatabaseChange> GetAllStoredChangesAsync() => _changes;

    private async IAsyncEnumerable<IDatabaseChange> GetAllChangesAsync(
        [EnumeratorCancellation] CancellationToken cancellation)
    {
        using var stream = new StreamReader(_changesFileName);

        var line = await stream.ReadLineAsync(cancellation);
        while (line is not null)
        {
            var change = JsonSerializer.Deserialize<IDatabaseChange>(line, JsonSerializationOptions.Default)!;
            line = await stream.ReadLineAsync(cancellation);

            yield return change;
        }
    }
}