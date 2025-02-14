using OneOf;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.Core.Storage;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;

namespace RedisV2.Database.Domain.Services.Storage;

public interface IDatabaseCollection
{
    OneOf<CollectionElement, UnexpectedError> Upsert(
        string key,
        CollectionElement element);

    OneOf<string, NotFoundError> Get(string key);

    IReadOnlyDictionary<string, CollectionElement> GetAll();

    OneOf<SuccessResult, UnexpectedError> Delete(string key);

    OneOf<SuccessResult, UnexpectedError> Flush();

    OneOf<SuccessResult, UnexpectedError> ApplyChanges(IDatabaseChange[] changes);
}