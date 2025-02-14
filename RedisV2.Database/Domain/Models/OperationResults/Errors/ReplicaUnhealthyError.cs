namespace RedisV2.Database.Domain.Models.OperationResults.Errors;

public record ReplicaUnhealthyError(string Message) : IError;