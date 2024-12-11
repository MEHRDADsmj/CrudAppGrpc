using Grpc.Core;
using CrudAppGrpc.Data;
using Microsoft.EntityFrameworkCore;

namespace CrudAppGrpc.Services;

public class CrudService : CrudAppGrpc.CrudService.CrudServiceBase
{
    private readonly ApplicationDbContext dbContext;
    private readonly ILogger<CrudService> _logger;
    public CrudService(ILogger<CrudService> logger, ApplicationDbContext context)
    {
        _logger = logger;
        dbContext = context;
    }

    public async override Task<CreateItemResponse> CreateItem(CreateItemRequest request, ServerCallContext context)
    {
        var item = new Item 
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
        };

        await dbContext.Items.AddAsync(item);
        await dbContext.SaveChangesAsync();
        return await Task.FromResult(new CreateItemResponse { Id = item.Id });
    }

    public async override Task<ReadItemResponse> ReadItem(ReadItemRequest request, ServerCallContext context)
    {
        var item = await dbContext.Items.SingleOrDefaultAsync(item => item.Id == request.Id);
        if (item != null)
        {
            return await Task.FromResult(new ReadItemResponse { Item = item });
        }
        throw new RpcException(new Status(StatusCode.NotFound, "Item not found"));
    }

    public async override Task<UpdateItemResponse> UpdateItem(UpdateItemRequest request, ServerCallContext context)
    {
        var item = await dbContext.Items.FirstAsync(item => item.Id == request.Id);
        if (item != null)
        {
            item.Name = request.Name;
            item.Description = request.Description;
            await dbContext.SaveChangesAsync();
            return await Task.FromResult(new UpdateItemResponse { Success = true});
        }
        return await Task.FromResult(new UpdateItemResponse { Success = false });
    }

    public async override Task<DeleteItemResponse> DeleteItem(DeleteItemRequest request, ServerCallContext context)
    {
        var item = await dbContext.Items.FirstAsync(item => item.Id == request.Id);
        dbContext.Items.Remove(item);
        await dbContext.SaveChangesAsync();
        return await Task.FromResult(new DeleteItemResponse { Success = true });
    }

    public override Task<ReturnAllResponse> ReturnAll(ReturnAllRequest request, ServerCallContext context)
    {
        var resp = new ReturnAllResponse();
        foreach (var item in dbContext.Items)
        {
            resp.Items.Add(item);
        }

        return Task.FromResult(resp);
    }
}
