namespace RedisV2.Database.Domain.Models.OperationResults.Errors;

public interface IError
{
    public string Message { get; }
}