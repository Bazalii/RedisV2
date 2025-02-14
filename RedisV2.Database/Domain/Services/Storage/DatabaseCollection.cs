using System.Collections.Concurrent;
using OneOf;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.Core.Storage;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using static RedisV2.Database.Helpers.OperationResults;

namespace RedisV2.Database.Domain.Services.Storage;

public class DatabaseCollection : IDatabaseCollection
{
    private ConcurrentDictionary<string, CollectionElement> _elements = new();

    public OneOf<CollectionElement, UnexpectedError> Upsert(
        string key,
        CollectionElement element)
    {
        try
        {
            _elements[key] = element;

            return element;
        }
        catch (Exception exception)
        {
            return new UnexpectedError(exception.Message);
        }
    }

    public OneOf<string, NotFoundError> Get(string key) =>
        _elements.TryGetValue(key, out var element)
            ? element.Value
            : new NotFoundError("Key not found");

    public IReadOnlyDictionary<string, CollectionElement> GetAll() => _elements;

    public OneOf<SuccessResult, UnexpectedError> Delete(string key) =>
        WithOperationStatus(
            () => { _elements.Remove(key, out _); });

    public OneOf<SuccessResult, UnexpectedError> Flush() =>
        WithOperationStatus(
            () => _elements = new ConcurrentDictionary<string, CollectionElement>());

    public OneOf<SuccessResult, UnexpectedError> ApplyChanges(IDatabaseChange[] changes) =>
        WithOperationStatus(
            () =>
            {
                foreach (var change in changes)
                {
                    switch (change)
                    {
                        case ElementDeletion deletion:
                            Delete(deletion.Key);
                            break;
                        case ElementUpsert upsert:
                            Upsert(upsert.Key, upsert.Element);
                            break;
                    }
                }
            });
}