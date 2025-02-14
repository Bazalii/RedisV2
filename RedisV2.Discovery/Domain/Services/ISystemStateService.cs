using CommonLibrary.Models.Enums;
using CommonLibrary.Models.Registration;

namespace RedisV2.Discovery.Domain.Services;

public interface ISystemStateService
{
    Task<(NodeRole nodeRole, int id)> RegisterAsync(RegistrationRequest request);
    string GetReplicaAddress();
    string GetLeaderAddress();
    void IncrementChangesCounter();
    void MakeReplicaInconsistent(int id);
    void MakeReplicaHealthy(int id);
    void DeleteUnavailableReplica(int id);
}