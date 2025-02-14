using CommonLibrary.Models.Enums;
using CommonLibrary.Models.Registration;
using Microsoft.Extensions.Options;
using RedisV2.Database.Domain.ServiceClients;
using RedisV2.Database.Infrastructure.Models;

namespace RedisV2.Database.Domain.Services.Registration;

public class DiscoveryService(
    IOptions<ServiceSettings> serviceSettings,
    IOptions<ServiceDiscoverySettings> serviceDiscoverySettings,
    DiscoveryServiceClient discoveryServiceClient)
    : IDiscoveryService
{
    // private readonly string _nodeIdFileName = serviceDiscoverySettings.Value.NodeIdFileName;

    private NodeRole _currentNodeRole;

    public async Task RegisterAsync(long lastChangeId)
    {
        // var currentNodeId = await GetNodeIdAsync();

        var registrationRequest = new RegistrationRequest
        {
            Name = serviceSettings.Value.Name,
            Port = serviceSettings.Value.Port,
            LastSavedChangeId = lastChangeId,
        };

        var (role, id) = await discoveryServiceClient.RegisterAsync(registrationRequest);

        _currentNodeRole = role;

        // if (currentNodeId is null)
        // {
        //     await SaveNodeIdAsync(id);
        // }
    }

    public bool IsNodeLeader() => _currentNodeRole is NodeRole.Leader;
    public bool IsNodeReplica() => _currentNodeRole is NodeRole.Replica;

    public void MakeLeader() => _currentNodeRole = NodeRole.Leader;

    // private async Task<int?> GetNodeIdAsync()
    // {
    //     using var stream = new StreamReader(_nodeIdFileName);
    //
    //     var line = await stream.ReadLineAsync();
    //     if (line is null)
    //     {
    //         return null;
    //     }
    //
    //     return int.TryParse(line, out var result)
    //         ? result
    //         : null;
    // }

    // private Task SaveNodeIdAsync(int nodeId) =>
    //     File.WriteAllTextAsync(_nodeIdFileName, nodeId.ToString());
}