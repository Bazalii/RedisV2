namespace RedisV2.Database.Domain.Models.OperationResults.Errors;

public record NotFoundError(string Message) : IError;