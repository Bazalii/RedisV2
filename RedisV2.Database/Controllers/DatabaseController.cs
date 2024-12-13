using System.Collections.Concurrent;
using Database;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RedisV2.Database.Domain.Models.OperationResults.Errors;
using RedisV2.Database.Domain.Services.Storage;
using RedisV2.Database.Helpers;

namespace RedisV2.Database.Controllers;

public class DatabaseController(
    IDatabase database)
    : DatabaseService.DatabaseServiceBase
{
    public override Task<OperationStatusResponse> AddCollection(
        AddCollectionRequest request,
        ServerCallContext context)
    {
        var addCollectionResult = database.AddCollection(request.Name);
        if (addCollectionResult.IsNotSuccess())
        {
            return GrpcResultsMapper.CreateErrorOperationStatusResponse((IError)addCollectionResult.Value);
        }

        return GrpcResultsMapper.CreateDefaultSuccessResponse();
    }

    public override Task<OperationStatusResponse> DeleteCollection(
        DeleteCollectionRequest request,
        ServerCallContext context)
    {
        var deleteCollectionResult = database.DeleteCollection(request.Name);
        if (deleteCollectionResult.IsNotSuccess())
        {
            return GrpcResultsMapper.CreateErrorOperationStatusResponse((IError)deleteCollectionResult.Value);
        }

        return GrpcResultsMapper.CreateDefaultSuccessResponse();
    }

    public override Task<OperationStatusResponse> UpsertElement(
        UpsertElementRequest request,
        ServerCallContext context)
    {
        var getCollectionResult = database.GetCollection(request.CollectionName);
        if (getCollectionResult.IsNotSuccess())
        {
            return GrpcResultsMapper.CreateErrorOperationStatusResponse((IError)getCollectionResult.Value);
        }

        var collection = (IDatabaseCollection)getCollectionResult.Value;
        var upsertElementResult = collection.Upsert(
            request.Key, request.Value, request.Expiry?.ToTimeSpan());
        if (upsertElementResult.IsNotSuccess())
        {
            return GrpcResultsMapper.CreateErrorOperationStatusResponse((IError)upsertElementResult.Value);
        }

        return GrpcResultsMapper.CreateDefaultSuccessResponse();
    }

    public override Task<GetElementResponse> GetElement(
        GetElementRequest request,
        ServerCallContext context)
    {
        var getCollectionResult = database.GetCollection(request.CollectionName);
        if (getCollectionResult.IsNotSuccess())
        {
            return Task.FromResult(
                new GetElementResponse
                {
                    Error = GrpcResultsMapper.CreateErrorResult((IError)getCollectionResult.Value)
                });
        }

        var collection = (IDatabaseCollection)getCollectionResult.Value;
        var upsertElementResult = collection.Get(request.Key);
        if (upsertElementResult.IsNotSuccess())
        {
            return Task.FromResult(
                new GetElementResponse
                {
                    Error = GrpcResultsMapper.CreateErrorResult((IError)upsertElementResult.Value)
                });
        }

        return Task.FromResult(
            new GetElementResponse
            {
                Element = new ElementResponse
                {
                    Value = (string)upsertElementResult.Value
                }
            });
    }

    public override Task<OperationStatusResponse> DeleteElement(
        DeleteElementRequest request,
        ServerCallContext context)
    {
        var getCollectionResult = database.GetCollection(request.CollectionName);
        if (getCollectionResult.IsNotSuccess())
        {
            return GrpcResultsMapper.CreateErrorOperationStatusResponse((IError)getCollectionResult.Value);
        }

        var collection = (IDatabaseCollection)getCollectionResult.Value;
        var upsertElementResult = collection.Delete(request.Key);
        if (upsertElementResult.IsNotSuccess())
        {
            return GrpcResultsMapper.CreateErrorOperationStatusResponse((IError)upsertElementResult.Value);
        }

        return GrpcResultsMapper.CreateDefaultSuccessResponse();
    }

    public override Task<OperationStatusResponse> FlushCollection(
        FlushCollectionRequest request,
        ServerCallContext context)
    {
        var flushCollectionResult = database.FlushCollection(request.Name);
        if (flushCollectionResult.IsNotSuccess())
        {
            return GrpcResultsMapper.CreateErrorOperationStatusResponse((IError)flushCollectionResult.Value);
        }

        return GrpcResultsMapper.CreateDefaultSuccessResponse();
    }

    public override Task<OperationStatusResponse> FlushDatabase(
        Empty request,
        ServerCallContext context)
    {
        var flushDatabaseResult = database.Flush();
        if (flushDatabaseResult.IsNotSuccess())
        {
            return GrpcResultsMapper.CreateErrorOperationStatusResponse((IError)flushDatabaseResult.Value);
        }

        return GrpcResultsMapper.CreateDefaultSuccessResponse();
    }
}