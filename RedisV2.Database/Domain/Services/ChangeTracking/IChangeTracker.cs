using OneOf;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;

namespace RedisV2.Database.Domain.Services.ChangeTracking;

public interface IChangeTracker
{
    Task LoadAllChangesAsync(CancellationToken cancellation);
    Task<OneOf<SuccessResult, UnexpectedError>> AddChangeToLogAsync(IDatabaseChange databaseChange);
    long GetLastChangeId();
    long GetNextChangeId();
    IDatabaseChange GetChangeById(long id);
    IReadOnlyCollection<IDatabaseChange> GetAllStoredChangesAsync();
}