using System.Text;
using System.Text.Json;
using CommonLibrary.Serialization;
using OneOf;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Models.OperationResults.SuccessResults;
using RedisV2.Database.Domain.Models.Views;

namespace RedisV2.Database.Domain.ServiceClients;

public class ReplicaServiceClient(HttpClient httpClient)
{
    public async Task<OneOf<SuccessResult, ReplicaUnhealthyError, ReplicaUnavailableError>> NotifyAboutChangeAsync(
        string replicaAddress,
        HandleChangeRequest handleChangeRequest)
    {
        const string method = "database/handle-change";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{replicaAddress}/{method}");

        request.Content =
            new StringContent(
                JsonSerializer.Serialize(
                    handleChangeRequest,
                    JsonSerializationOptions.Default),
                Encoding.UTF8,
                "application/json");

        try
        {
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode is false)
            {
                return new ReplicaUnhealthyError(
                    $"Replica request failed, status code: {response.IsSuccessStatusCode}");
            }
        }
        catch (Exception exception)
        {
            return new ReplicaUnavailableError(exception.Message);
        }

        return new SuccessResult();
    }
}