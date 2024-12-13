namespace RedisV2.Database.Domain.Models.OperationResults.Errors;

public record AlreadyExistsError(string Message) : IError;