using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class PurchaseOrderRepository : CrudRepository<PurchaseOrderDocument>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(IMongoClient client)
        : base(client, "Inventory", "PurchaseOrder") { }

    public new async Task<PurchaseOrder?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : PurchaseOrderDocumentMapper.ToEntity(document);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByBranchAsync(string branchReference, int page, int pageSize)
    {
        var documents = await _collection
            .Find(po => po.BranchReference == branchReference)
            .SortByDescending(po => po.OrderDateUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(PurchaseOrderDocumentMapper.ToEntity);
    }

    public async Task<long> CountByBranchAsync(string? branchReference)
    {
        var filter = string.IsNullOrEmpty(branchReference)
            ? FilterDefinition<PurchaseOrderDocument>.Empty
            : Builders<PurchaseOrderDocument>.Filter.Eq(po => po.BranchReference, branchReference);

        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<IEnumerable<PurchaseOrder>> SearchAsync(
        string? branchReference,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    )
    {
        var filters = new List<FilterDefinition<PurchaseOrderDocument>>();

        if (!string.IsNullOrEmpty(branchReference))
        {
            filters.Add(Builders<PurchaseOrderDocument>.Filter.Eq(po => po.BranchReference, branchReference));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<PurchaseOrderDocument>.Filter.Gte(po => po.OrderDateUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<PurchaseOrderDocument>.Filter.Lte(po => po.OrderDateUtc, to.Value));
        }

        var filter =
            filters.Count > 0
                ? Builders<PurchaseOrderDocument>.Filter.And(filters)
                : FilterDefinition<PurchaseOrderDocument>.Empty;

        var documents = await _collection
            .Find(filter)
            .SortByDescending(po => po.OrderDateUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(PurchaseOrderDocumentMapper.ToEntity);
    }

    public async Task<int> CountAsync(string? branchReference, DateTime? from, DateTime? to)
    {
        var filters = new List<FilterDefinition<PurchaseOrderDocument>>();

        if (!string.IsNullOrEmpty(branchReference))
        {
            filters.Add(Builders<PurchaseOrderDocument>.Filter.Eq(po => po.BranchReference, branchReference));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<PurchaseOrderDocument>.Filter.Gte(po => po.OrderDateUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<PurchaseOrderDocument>.Filter.Lte(po => po.OrderDateUtc, to.Value));
        }

        var filter =
            filters.Count > 0
                ? Builders<PurchaseOrderDocument>.Filter.And(filters)
                : FilterDefinition<PurchaseOrderDocument>.Empty;

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<PurchaseOrder> CreateAsync(PurchaseOrder purchaseOrder)
    {
        var document = PurchaseOrderDocumentMapper.ToDocument(purchaseOrder);
        var result = await UpsertAsync(null, document);
        return PurchaseOrderDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(PurchaseOrder purchaseOrder)
    {
        var document = PurchaseOrderDocumentMapper.ToDocument(purchaseOrder);
        await UpsertAsync(purchaseOrder.Id, document);
    }
}
