namespace RedisV2.Database.Domain.Services.Registration;

public interface IDiscoveryService
{
    Task RegisterAsync(long lastChangeId);
    bool IsNodeLeader();
    bool IsNodeReplica();
    void MakeLeader();
}