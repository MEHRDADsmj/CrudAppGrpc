using System.Collections.Concurrent;
using Grpc.Core;

namespace CrudAppGrpc.Services;

public class CrudService : CrudAppGrpc.CrudService.CrudServiceBase
{
    private readonly Dictionary<string, Item> database = new();
    private readonly ILogger<CrudService> _logger;
    public CrudService(ILogger<CrudService> logger)
    {
        _logger = logger;
    }

    public override Task<CreateItemResponse> CreateItem(CreateItemRequest request, ServerCallContext context)
    {
        var id = Guid.NewGuid().ToString();
        var item = new Item 
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
        };

        database.TryAdd(id, item);
        return Task.FromResult(new CreateItemResponse { Id = id });
    }

    public override Task<ReadItemResponse> ReadItem(ReadItemRequest request, ServerCallContext context)
    {
        var item = database.SingleOrDefault(id => id.Key == request.Id).Value;
        if (item != null)
        {
            return Task.FromResult(new ReadItemResponse { Item = item });
        }
        throw new RpcException(new Status(StatusCode.NotFound, "Item not found"));
    }

    public override Task<UpdateItemResponse> UpdateItem(UpdateItemRequest request, ServerCallContext context)
    {
        if (database.TryGetValue(request.Id, out var item))
        {
            item.Name = request.Name;
            item.Description = request.Description;
            return Task.FromResult(new UpdateItemResponse { Success = true});
        }
        return Task.FromResult(new UpdateItemResponse { Success = false });
    }

    public override Task<DeleteItemResponse> DeleteItem(DeleteItemRequest request, ServerCallContext context)
    {
        var success = database.Remove(request.Id, out _);
        return Task.FromResult(new DeleteItemResponse { Success = success });
    }
}
