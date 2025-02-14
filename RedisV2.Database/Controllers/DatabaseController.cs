using Microsoft.AspNetCore.Mvc;
using RedisV2.Database.Domain.Models.Views;
using RedisV2.Database.Domain.Services.Storage;
using RedisV2.Database.Integration.Models.Requests;
using RedisV2.Database.Integration.Models.Responses;

namespace RedisV2.Database.Controllers;

[ApiController]
[Route("database")]
public class DatabaseController(
    IDatabaseService databaseService)
    : ControllerBase
{
    [HttpPost("collection")]
    public async Task<ActionResult> AddCollection(AddCollectionRequest request)
    {
        var addCollectionResult = await databaseService.AddCollection(request.Name);

        return addCollectionResult.Match<ActionResult>(
            success => Ok(),
            alreadyExistsError => Conflict(alreadyExistsError.Message),
            unexpectedError => Problem(unexpectedError.Message));
    }

    [HttpDelete("collection")]
    public async Task<ActionResult> DeleteCollection(DeleteCollectionRequest request)
    {
        var deleteCollectionResult = await databaseService.DeleteCollection(request.Name);

        return deleteCollectionResult.Match<ActionResult>(
            success => Ok(),
            unexpectedError => Problem(unexpectedError.Message));
    }

    [HttpPost("element")]
    public async Task<ActionResult> UpsertElement(UpsertElementRequest request)
    {
        var upsertElementResult = await databaseService.UpsertElement(
            request.CollectionName,
            request.Key,
            request.Value,
            request.Expiry);

        return upsertElementResult.Match<ActionResult>(
            success => Ok(),
            notFoundError => NotFound(notFoundError.Message),
            unexpectedError => Problem(unexpectedError.Message));
    }

    [HttpGet("element")]
    public ActionResult<GetElementResponse> GetElement(
        string collectionName,
        string key)
    {
        var getElementResult = databaseService.GetElement(
            collectionName, key);

        return getElementResult.Match<ActionResult>(
            success => Ok(new GetElementResponse
            {
                Value = success,
            }),
            notFoundError => NotFound(notFoundError.Message),
            unexpectedError => Problem(unexpectedError.Message));
    }

    [HttpDelete("element")]
    public async Task<ActionResult> DeleteElement(DeleteElementRequest request)
    {
        var getElementResult = await databaseService.DeleteElement(
            request.CollectionName, request.Key);

        return getElementResult.Match<ActionResult>(
            success => Ok(),
            notFoundError => NotFound(notFoundError.Message),
            unexpectedError => Problem(unexpectedError.Message));
    }

    [HttpPost("handle-change")]
    public async Task<ActionResult> HandleChange(HandleChangeRequest request)
    {
        var applyChangeResult = await databaseService.ApplyChange(request.Change);

        return applyChangeResult.Match<ActionResult>(
            success => Ok(),
            unexpectedError => Problem(unexpectedError.Message));
    }

    [HttpGet("last-change-id")]
    public ActionResult GetLastChangeId()
    {
        var lastChangeId = databaseService.GetLastChangeId();

        return Ok(lastChangeId);
    }

    [HttpDelete("flush-collection")]
    public async Task<ActionResult> FlushCollection(FlushCollectionRequest request)
    {
        var flushCollectionResult = await databaseService.FlushCollection(request.Name);

        return flushCollectionResult.Match<ActionResult>(
            success => Ok(),
            notFoundError => NotFound(notFoundError.Message),
            unexpectedError => Problem(unexpectedError.Message));
    }

    [HttpDelete("flush-database")]
    public async Task<ActionResult> FlushDatabase()
    {
        var flushDatabaseResult = await databaseService.Flush();

        return flushDatabaseResult.Match<ActionResult>(
            success => Ok(),
            unexpectedError => Problem(unexpectedError.Message));
    }
}