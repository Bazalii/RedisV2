using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using OneOf;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;

namespace RedisV2.Database.Domain.Services.Storage;

public interface IDatabaseService
{
    Task InitAsync(CancellationToken cancellation);

    Task<OneOf<SuccessResult, AlreadyExistsError, UnexpectedError>> AddCollection(string collectionName);
    OneOf<IDatabaseCollection, NotFoundError, UnexpectedError> GetCollection(string collectionName);
    Task<OneOf<SuccessResult, UnexpectedError>> DeleteCollection(string collectionName);
    Task<OneOf<SuccessResult, NotFoundError, UnexpectedError>> FlushCollection(string collectionName);
    Task<OneOf<SuccessResult, UnexpectedError>> Flush();

    Task<OneOf<SuccessResult, NotFoundError, UnexpectedError>> UpsertElement(
        string collectionName,
        string key,
        string value,
        TimeSpan? expiry);

    OneOf<string, NotFoundError, UnexpectedError> GetElement(
        string collectionName,
        string key);

    Task<OneOf<SuccessResult, NotFoundError, UnexpectedError>> DeleteElement(
        string collectionName,
        string key);

    Task<OneOf<SuccessResult, UnexpectedError>> ApplyChange(IDatabaseChange change);
    long GetLastChangeId();
    IDatabaseChange GetChangeById(long id);
    void StartCleanupTimer();
}