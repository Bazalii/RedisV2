namespace RedisV2.Database.Domain.Models.OperationResults.Errors;

public record UnexpectedError(string Message) : IError;