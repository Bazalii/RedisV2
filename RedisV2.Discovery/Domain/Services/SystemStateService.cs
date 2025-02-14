using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CommonLibrary.Models.Enums;
using CommonLibrary.Models.Nodes;
using CommonLibrary.Models.NodeStateChanges;
using CommonLibrary.Models.Registration;
using RedisV2.Discovery.Domain.NodeClients;

namespace RedisV2.Discovery.Domain.Services;

public class SystemStateService(
    LeaderServiceClient leaderServiceClient,
    ReplicaServiceClient replicaServiceClient,
    ILogger<SystemStateService> logger) : ISystemStateService
{
    private bool _isLeaderChanging;
    private int _currentNodeId;

    private Node? _leader;

    private readonly ConcurrentDictionary<int, Node> _healthyReplicas = [];
    private readonly ConcurrentDictionary<int, Node> _inconsistentReplicas = [];

    private readonly Lock _replicaIdLock = new();
    private int _currentReplicaId = 0;

    private long _lastChangeId = 0;

    public async Task<(NodeRole nodeRole, int id)> RegisterAsync(RegistrationRequest request)
    {
        if (_isLeaderChanging)
        {
            throw new InvalidOperationException("Cannot register because leader is changing");
        }

        var nodeId = GetNextNodeId();
        var node = CreateNode(nodeId, request.Name, request.Port);

        if (IsLeaderAlreadyRegistered() is false)
        {
            var registrationResult = Interlocked.CompareExchange(ref _leader, node, null);
            if (registrationResult is null)
            {
                logger.LogInformation($"Leader with id: {nodeId} is registered");

                _lastChangeId = request.LastSavedChangeId;

                StartCheckingLeaderStatus();

                return (NodeRole.Leader, nodeId);
            }
        }

        if (request.LastSavedChangeId < _lastChangeId)
        {
            _inconsistentReplicas[nodeId] = node;
        }
        else
        {
            _healthyReplicas[nodeId] = node;
        }

        await NotifyLeaderAboutNewReplicaAsync(node, request.LastSavedChangeId);

        logger.LogInformation($"Replica with id: {nodeId} is registered");

        return (NodeRole.Replica, nodeId);
    }

    public string GetReplicaAddress()
    {
        string address;

        // If there are no healthy replicas, leader becomes available for reading
        if (_healthyReplicas.Count == 0)
        {
            return _leader?.Address ?? string.Empty;
        }

        lock (_replicaIdLock)
        {
            address = _healthyReplicas.ElementAt(_currentReplicaId).Value.Address;

            var newReplicaId = _currentReplicaId + 1;

            _currentReplicaId = newReplicaId < _healthyReplicas.Count
                ? newReplicaId
                : 0;
        }

        return address;
    }

    public string GetLeaderAddress()
    {
        if (_isLeaderChanging)
        {
            throw new InvalidOperationException("Cannot get leader address because leader is changing");
        }

        return _leader?.Address ?? string.Empty;
    }

    // todo записывать на диск
    public void IncrementChangesCounter() => Interlocked.Increment(ref _lastChangeId);

    public void MakeReplicaInconsistent(int id)
    {
        Node replica;

        lock (_replicaIdLock)
        {
            _currentReplicaId = 0;
            _healthyReplicas.Remove(id, out replica!);
        }

        _inconsistentReplicas[id] = replica;
    }

    public void MakeReplicaHealthy(int id)
    {
        _inconsistentReplicas.Remove(id, out var replica);
        _healthyReplicas[id] = replica!;
    }

    public void DeleteUnavailableReplica(int id)
    {
        if (_healthyReplicas.ContainsKey(id))
        {
            lock (_replicaIdLock)
            {
                _currentReplicaId = 0;
                _healthyReplicas.Remove(id, out _);
            }

            return;
        }

        _inconsistentReplicas.Remove(id, out _);
    }

    private bool IsLeaderAlreadyRegistered() => _leader is not null;
    private int GetNextNodeId() => Interlocked.Increment(ref _currentNodeId);

    private Node CreateNode(
        int nodeId,
        string name,
        int port) => new()
    {
        Id = nodeId,
        Address = CreateNodeAddress(name, port)
    };

    private static string CreateNodeAddress(string name, int port) => $"http://{name}:{port}";

    private Task NotifyLeaderAboutNewReplicaAsync(
        Node node,
        long lastSavedChangeId)
    {
        var leaderAddress = GetLeaderAddress();

        var addNewReplicaRequest = new AddNewReplicaRequest
        {
            Id = node.Id,
            Address = node.Address,
            LastSavedChangeId = lastSavedChangeId
        };

        return leaderServiceClient.NotifyLeaderAboutNewReplicaAsync(
            leaderAddress, addNewReplicaRequest);
    }

    private void StartCheckingLeaderStatus() =>
        Task.Factory.StartNew(CheckIfLeaderIsHealthy, TaskCreationOptions.LongRunning);

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    private async void CheckIfLeaderIsHealthy()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            logger.LogInformation("Checking if leader is healthy...");
            var leaderAddress = GetLeaderAddress();

            var isLeaderHealthy = await leaderServiceClient.IsLeaderHealthyAsync(leaderAddress);
            if (isLeaderHealthy is false)
            {
                await ChooseNewLeaderAsync();
            }
        }
    }

    private async Task ChooseNewLeaderAsync()
    {
        _isLeaderChanging = true;
        _leader!.Address = "";

        var newLeader = _healthyReplicas.Values.First();

        var healthyReplicas = _healthyReplicas.Values
            .Except([newLeader])
            .ToArray();
        var inconsistentReplicas = _inconsistentReplicas.Values.ToArray();

        var getLastSavedChangeIdTasks = inconsistentReplicas
            .Select(replica => replicaServiceClient.GetLastSavedChangeIdAsync(replica.Address))
            .ToArray();

        await Task.WhenAll(getLastSavedChangeIdTasks);

        var inconsistentReplicasRequests = new InconsistentNode[inconsistentReplicas.Length];

        for (var i = 0; i < inconsistentReplicas.Length; i++)
        {
            var replica = inconsistentReplicas[i];
            var lastSavedChangeId = getLastSavedChangeIdTasks[i].Result;
            var request = new InconsistentNode
            {
                Id = replica.Id,
                Address = replica.Address,
                LastChangeId = lastSavedChangeId
            };

            inconsistentReplicasRequests[i] = request;
        }

        var healthyReplicasRequests = healthyReplicas
            .Select(replica => new Node
            {
                Id = replica.Id,
                Address = replica.Address
            })
            .ToArray();

        var makeReplicaLeaderRequest = new MakeNodeLeaderRequest
        {
            HealthyReplicas = healthyReplicasRequests,
            InconsistentReplicas = inconsistentReplicasRequests
        };

        await replicaServiceClient.MakeNodeLeaderAsync(newLeader.Address, makeReplicaLeaderRequest);

        _leader = newLeader;

        lock (_replicaIdLock)
        {
            _currentReplicaId = 0;
            _healthyReplicas.Remove(newLeader.Id, out _);
        }

        _isLeaderChanging = false;
    }
}