namespace RedisV2.Database.Domain.Models.OperationResults.Errors;

public record ReplicaUnavailableError(string Message) : IError;