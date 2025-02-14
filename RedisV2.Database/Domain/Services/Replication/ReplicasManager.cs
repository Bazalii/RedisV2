using System.Collections.Concurrent;
using CommonLibrary.Models.Nodes;
using OneOf;
using RedisV2.Database.Domain.Models.Core.ChangeTracking;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using RedisV2.Database.Domain.Models.Views;
using RedisV2.Database.Domain.ServiceClients;
using RedisV2.Database.Domain.Services.ChangeTracking;

namespace RedisV2.Database.Domain.Services.Replication;

public class ReplicasManager(
    IChangeTracker changeTracker,
    ReplicaServiceClient replicaServiceClient,
    DiscoveryServiceClient discoveryServiceClient,
    ILogger<ReplicasManager> logger)
    : IReplicasManager
{
    private readonly ConcurrentDictionary<int, Node> _healthyReplicas = [];
    private readonly ConcurrentDictionary<int, InconsistentNode> _inconsistentReplicas = [];

    public async Task NotifyAllHealthyReplicasAboutChangeAsync(IDatabaseChange change)
    {
        var currentChangeId = change.Id;

        var notifyTasks = _healthyReplicas.Values.ToDictionary(
            replica => replica.Id,
            replica => NotifyAboutChangeAsync(replica.Address, change));

        await Task.WhenAll(notifyTasks.Values);

        foreach (var (nodeId, task) in notifyTasks)
        {
            var notificationResult = task.Result;

            switch (notificationResult.Value)
            {
                case SuccessResult:
                    continue;
                case ReplicaUnhealthyError:
                    var inconsistentReplica = await MakeReplicaInconsistent(
                        nodeId, currentChangeId - 1);
                    StartReplicaRecovering(inconsistentReplica);
                    continue;
                case ReplicaUnavailableError:
                    await DeleteUnavailableReplicaAsync(nodeId);
                    continue;
            }
        }

        await discoveryServiceClient.IncrementChangesCounterAsync();
    }

    private Task<OneOf<SuccessResult, ReplicaUnhealthyError, ReplicaUnavailableError>> NotifyAboutChangeAsync(
        string address,
        IDatabaseChange change)
    {
        return replicaServiceClient.NotifyAboutChangeAsync(
            address,
            new HandleChangeRequest
            {
                Change = change,
            });
    }

    public void AddReplica(
        int id,
        string address,
        long lastChangeId)
    {
        var currentLastChangeId = changeTracker.GetLastChangeId();

        if (currentLastChangeId > lastChangeId)
        {
            var inconsistentReplica = new InconsistentNode
            {
                Id = id,
                Address = address,
                LastChangeId = lastChangeId,
            };

            _inconsistentReplicas[id] = inconsistentReplica;
            
            StartReplicaRecovering(inconsistentReplica);

            return;
        }

        _healthyReplicas[id] = new Node
        {
            Id = id,
            Address = address,
        };
    }

    public void LoadReplicas(
        Node[] healthyReplicas,
        InconsistentNode[] inconsistentReplicas)
    {
        foreach (var healthyReplica in healthyReplicas)
        {
            _healthyReplicas[healthyReplica.Id] = healthyReplica;
        }

        foreach (var inconsistentReplica in inconsistentReplicas)
        {
            _inconsistentReplicas[inconsistentReplica.Id] = inconsistentReplica;
        }
    }

    private async Task<InconsistentNode> MakeReplicaInconsistent(
        int id,
        long lastChangeId)
    {
        _healthyReplicas.Remove(id, out var replica);

        var inconsistentReplica = new InconsistentNode
        {
            Id = replica!.Id,
            Address = replica.Address,
            LastChangeId = lastChangeId,
        };

        _inconsistentReplicas[id] = inconsistentReplica;

        await discoveryServiceClient.MakeReplicaInconsistentAsync(id);

        return inconsistentReplica;
    }

    private async Task MakeReplicaHealthy(int id)
    {
        _inconsistentReplicas.Remove(id, out var replica);

        _healthyReplicas[id] = new Node
        {
            Id = replica!.Id,
            Address = replica.Address,
        };

        await discoveryServiceClient.MakeReplicaHealthyAsync(id);
    }

    private void StartReplicaRecovering(InconsistentNode inconsistentReplica) =>
        Task.Factory.StartNew(() => RecoverReplica(inconsistentReplica));

    private async void RecoverReplica(InconsistentNode inconsistentReplica)
    {
        var nextChangeId = inconsistentReplica.LastChangeId + 1;

        while (nextChangeId <= changeTracker.GetLastChangeId())
        {
            var currentChange = changeTracker.GetChangeById(nextChangeId);

            logger.LogInformation($"Sending change{currentChange} to replica {inconsistentReplica.Id}");

            var notificationResult = await NotifyAboutChangeAsync(
                inconsistentReplica.Address, currentChange);
            switch (notificationResult.Value)
            {
                case ReplicaUnavailableError:
                    await DeleteUnavailableReplicaAsync(inconsistentReplica.Id);
                    return;
                case ReplicaUnhealthyError:
                    continue;
                case SuccessResult:
                    nextChangeId++;
                    break;
            }
        }

        await MakeReplicaHealthy(inconsistentReplica.Id);
    }

    private async Task DeleteUnavailableReplicaAsync(int id)
    {
        if (_healthyReplicas.ContainsKey(id))
        {
            _healthyReplicas.Remove(id, out _);
        }
        else
        {
            _inconsistentReplicas.Remove(id, out _);
        }

        await discoveryServiceClient.DeleteUnavailableReplicaAsync(id);
    }
}