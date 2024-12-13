using OneOf;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;

namespace RedisV2.Database.Domain.Services.Storage;

public interface IDatabaseCollection
{
    OneOf<SuccessResult, UnexpectedError> Upsert(
        string key,
        string value,
        TimeSpan? expiry);

    OneOf<string, NotFoundError> Get(string key);
    OneOf<SuccessResult, UnexpectedError> Delete(string key);

    OneOf<SuccessResult, UnexpectedError> Flush();

    OneOf<SuccessResult, UnexpectedError> ApplyChanges(ICollectionChange[] changes);
}