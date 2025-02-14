using CommonLibrary.Models.NodeStateChanges;
using Microsoft.AspNetCore.Mvc;
using RedisV2.Database.Domain.Services.Registration;
using RedisV2.Database.Domain.Services.Replication;

namespace RedisV2.Database.Controllers;

[ApiController]
public class ReplicationController(
    IReplicasManager replicasManager,
    IDiscoveryService discoveryService) : ControllerBase
{
    [HttpPost("add-replica")]
    public ActionResult AddReplica(AddNewReplicaRequest request)
    {
        replicasManager.AddReplica(
            request.Id, request.Address, request.LastSavedChangeId);

        return Ok();
    }

    [HttpPost("make-leader")]
    public ActionResult MakeLeader(MakeNodeLeaderRequest request)
    {
        replicasManager.LoadReplicas(
            request.HealthyReplicas, request.InconsistentReplicas);

        discoveryService.MakeLeader();

        return Ok();
    }
}