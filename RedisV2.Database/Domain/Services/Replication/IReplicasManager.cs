using CommonLibrary.Models.Nodes;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;

namespace RedisV2.Database.Domain.Services.Replication;

public interface IReplicasManager
{
    Task NotifyAllHealthyReplicasAboutChangeAsync(IDatabaseChange change);

    void AddReplica(
        int id,
        string address,
        long lastChangeId);

    void LoadReplicas(
        Node[] healthyReplicas,
        InconsistentNode[] inconsistentReplicas);
}