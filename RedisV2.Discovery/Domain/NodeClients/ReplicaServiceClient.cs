using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommonLibrary.Models.NodeStateChanges;
using CommonLibrary.Serialization;

namespace RedisV2.Discovery.Domain.NodeClients;

public class ReplicaServiceClient(HttpClient httpClient)
{
    public async Task<long> GetLastSavedChangeIdAsync(
        string replicaAddress)
    {
        const string method = "database/last-change-id";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{replicaAddress}/{method}");

        var response = await httpClient.SendAsync(request);

        return await response.Content.ReadFromJsonAsync<long>(JsonSerializationOptions.Default);
    }

    public Task MakeNodeLeaderAsync(
        string newLeaderAddress,
        MakeNodeLeaderRequest makeNodeLeaderRequest)
    {
        const string method = "make-leader";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{newLeaderAddress}/{method}");

        request.Content =
            new StringContent(
                JsonSerializer.Serialize(
                    makeNodeLeaderRequest,
                    JsonSerializationOptions.Default),
                Encoding.UTF8,
                "application/json");

        return httpClient.SendAsync(request);
    }
}