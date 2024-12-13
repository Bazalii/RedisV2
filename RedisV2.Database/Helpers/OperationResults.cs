using OneOf;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;

namespace RedisV2.Database.Helpers;

public static class OperationResults
{
    public static OneOf<SuccessResult, UnexpectedError> WithOperationStatus(Action action)
    {
        try
        {
            action();

            return new SuccessResult();
        }
        catch (Exception exception)
        {
            return new UnexpectedError(exception.Message);
        }
    }

    public static bool IsNotSuccess(this IOneOf result)
    {
        return result.Value is
            AlreadyExistsError or
            NotFoundError or
            UnexpectedError;
    }
}