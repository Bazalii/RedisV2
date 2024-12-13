using Database;
using Grpc.Core;
using RedisV2.Database.Domain.Models.OperationResults.Errors;

namespace RedisV2.Database.Helpers;

public static class GrpcResultsMapper
{
    public static Task<OperationStatusResponse> CreateDefaultSuccessResponse()
    {
        var response = CreateSuccessOperationStatusResponse((int)StatusCode.OK, "Success");

        return Task.FromResult(response);
    }

    private static OperationStatusResponse CreateSuccessOperationStatusResponse(int status, string message) =>
        new()
        {
            SuccessResult = new SuccessResponse
            {
                Status = status,
                Message = message
            }
        };

    public static Task<OperationStatusResponse> CreateErrorOperationStatusResponse(IError error)
    {
        var response = new OperationStatusResponse
        {
            ErrorResult = CreateErrorResult(error)
        };

        return Task.FromResult(response);
    }

    public static ErrorResponse CreateErrorResult(IError error) =>
        error switch
        {
            AlreadyExistsError alreadyExistsError =>
                CreateOperationResult((int)StatusCode.AlreadyExists, alreadyExistsError.Message),
            NotFoundError notFoundError =>
                CreateOperationResult((int)StatusCode.NotFound, notFoundError.Message),
            UnexpectedError unexpectedError =>
                CreateOperationResult((int)StatusCode.Internal, unexpectedError.Message),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
        };

    private static ErrorResponse CreateOperationResult(int status, string message) =>
        new()
        {
            Status = status,
            Message = message
        };
}