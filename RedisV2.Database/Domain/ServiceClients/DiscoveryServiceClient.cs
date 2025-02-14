using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CommonLibrary.Models.Enums;
using CommonLibrary.Models.Registration;
using CommonLibrary.Serialization;
using Microsoft.Extensions.Options;
using RedisV2.Database.Infrastructure.Models;

namespace RedisV2.Database.Domain.ServiceClients;

public class DiscoveryServiceClient(
    IOptions<ServiceDiscoverySettings> serviceDiscoverySettings,
    HttpClient httpClient)
{
    private readonly string _discoveryServiceAddress = serviceDiscoverySettings.Value.ServiceAddress;

    public async Task<(NodeRole role, int id)> RegisterAsync(RegistrationRequest registrationRequest)
    {
        const string method = "register";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_discoveryServiceAddress}/{method}");

        request.Content =
            new StringContent(
                JsonSerializer.Serialize(
                    registrationRequest,
                    JsonSerializationOptions.Default),
                Encoding.UTF8,
                "application/json");

        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode is false)
        {
            throw new UnreachableException($"Service discovery was unreachable: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<RegistrationResponse>();

        return (result!.NodeRole, result.Id);
    }

    public Task IncrementChangesCounterAsync()
    {
        const string method = "increment-changes-counter";
        var request = new HttpRequestMessage(HttpMethod.Put, $"{_discoveryServiceAddress}/{method}");

        return httpClient.SendAsync(request);
    }

    public Task MakeReplicaInconsistentAsync(int id)
    {
        var method = $"make-replica-inconsistent/{id}";
        var request = new HttpRequestMessage(HttpMethod.Put, $"{_discoveryServiceAddress}/{method}");

        return httpClient.SendAsync(request);
    }

    public Task MakeReplicaHealthyAsync(int id)
    {
        var method = $"make-replica-healthy/{id}";
        var request = new HttpRequestMessage(HttpMethod.Put, $"{_discoveryServiceAddress}/{method}");

        return httpClient.SendAsync(request);
    }

    public Task DeleteUnavailableReplicaAsync(int id)
    {
        var method = $"replica/{id}";
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_discoveryServiceAddress}/{method}");

        return httpClient.SendAsync(request);
    }
}