using OneOf;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;

namespace RedisV2.Database.Domain.Services.Storage;

public interface IDatabase
{
    OneOf<SuccessResult, AlreadyExistsError, UnexpectedError> AddCollection(string collectionName);
    OneOf<IDatabaseCollection, NotFoundError, UnexpectedError> GetCollection(string collectionName);
    OneOf<SuccessResult, UnexpectedError> DeleteCollection(string collectionName);
    OneOf<SuccessResult, NotFoundError, UnexpectedError> FlushCollection(string collectionName);
    OneOf<SuccessResult, UnexpectedError> Flush();
}