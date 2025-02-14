using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CommonLibrary.Models.NodeStateChanges;
using CommonLibrary.Serialization;

namespace RedisV2.Discovery.Domain.NodeClients;

public class LeaderServiceClient(HttpClient httpClient)
{
    public async Task NotifyLeaderAboutNewReplicaAsync(
        string leaderAddress,
        AddNewReplicaRequest addNewReplicaRequest)
    {
        const string method = "add-replica";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{leaderAddress}/{method}");

        request.Content =
            new StringContent(
                JsonSerializer.Serialize(
                    addNewReplicaRequest,
                    JsonSerializationOptions.Default),
                Encoding.UTF8,
                "application/json");

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode is false)
        {
            throw new UnreachableException($"Replica was not added. Master status code: {response.StatusCode}");
        }
    }

    public async Task<bool> IsLeaderHealthyAsync(string leaderAddress)
    {
        const string method = "health-check";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{leaderAddress}/{method}");

        try
        {
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode is false)
            {
                return false;
            }
        }
        catch (Exception exception)
        {
            return false;
        }

        return true;
    }
}