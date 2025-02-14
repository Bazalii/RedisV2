using CommonLibrary.Models.Registration;
using Microsoft.AspNetCore.Mvc;
using RedisV2.Discovery.Domain.Services;
using RedisV2.Discovery.Integration.Models.Views;

namespace RedisV2.Discovery.Controllers;

[ApiController]
public class DiscoveryController(
    ISystemStateService systemStateService)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponse>> Register(RegistrationRequest request)
    {
        var (role, id) = await systemStateService.RegisterAsync(request);

        return new RegistrationResponse
        {
            Id = id,
            NodeRole = role
        };
    }

    [HttpGet("reading-address")]
    public ActionResult<AddressResponse> GetAddressForReading()
    {
        var address = systemStateService.GetReplicaAddress();

        return new AddressResponse
        {
            Address = address
        };
    }

    [HttpGet("writing-address")]
    public ActionResult<AddressResponse> GetAddressForWriting()
    {
        var address = systemStateService.GetLeaderAddress();

        return new AddressResponse
        {
            Address = address
        };
    }

    [HttpPut("increment-changes-counter")]
    public ActionResult IncrementChangesCounter()
    {
        systemStateService.IncrementChangesCounter();

        return Ok();
    }

    [HttpPut("make-replica-inconsistent/{id:int}")]
    public ActionResult MakeReplicaInconsistent(int id)
    {
        systemStateService.MakeReplicaInconsistent(id);

        return Ok();
    }

    [HttpPut("make-replica-healthy/{id:int}")]
    public ActionResult MakeReplicaHealthy(int id)
    {
        systemStateService.MakeReplicaHealthy(id);

        return Ok();
    }

    [HttpDelete("replica/{id:int}")]
    public ActionResult DeleteUnavailableReplica(int id)
    {
        systemStateService.DeleteUnavailableReplica(id);

        return Ok();
    }
}