using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class InventoryAdjustmentRepository : CrudRepository<InventoryAdjustmentDocument>, IInventoryAdjustmentRepository
{
    public InventoryAdjustmentRepository(IMongoClient client)
        : base(client, "Inventory", "InventoryAdjustment") { }

    public new async Task<InventoryAdjustment?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : InventoryAdjustmentDocumentMapper.ToEntity(document);
    }

    public async Task<IEnumerable<InventoryAdjustment>> SearchAsync(
        string? branchReference,
        AdjustmentType? adjustmentType,
        InventoryAdjustmentStatus? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    )
    {
        var filter = BuildSearchFilter(branchReference, adjustmentType, status, from, to);

        var documents = await _collection
            .Find(filter)
            .SortByDescending(a => a.AdjustmentDateUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(InventoryAdjustmentDocumentMapper.ToEntity);
    }

    public async Task<int> CountAsync(
        string? branchReference,
        AdjustmentType? adjustmentType,
        InventoryAdjustmentStatus? status,
        DateTime? from,
        DateTime? to
    )
    {
        var filter = BuildSearchFilter(branchReference, adjustmentType, status, from, to);
        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<InventoryAdjustment> CreateAsync(InventoryAdjustment adjustment)
    {
        var document = InventoryAdjustmentDocumentMapper.ToDocument(adjustment);
        var result = await UpsertAsync(null, document);
        return InventoryAdjustmentDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(InventoryAdjustment adjustment)
    {
        var document = InventoryAdjustmentDocumentMapper.ToDocument(adjustment);
        await UpsertAsync(adjustment.Id, document);
    }

    private static FilterDefinition<InventoryAdjustmentDocument> BuildSearchFilter(
        string? branchReference,
        AdjustmentType? adjustmentType,
        InventoryAdjustmentStatus? status,
        DateTime? from,
        DateTime? to
    )
    {
        var filters = new List<FilterDefinition<InventoryAdjustmentDocument>>();

        if (!string.IsNullOrEmpty(branchReference))
        {
            // Match adjustments where any branch field references this branch
            var branchFilter = Builders<InventoryAdjustmentDocument>.Filter.Or(
                Builders<InventoryAdjustmentDocument>.Filter.Eq(a => a.BranchReference, branchReference),
                Builders<InventoryAdjustmentDocument>.Filter.Eq(a => a.OriginBranchReference, branchReference),
                Builders<InventoryAdjustmentDocument>.Filter.Eq(a => a.DestinationBranchReference, branchReference)
            );
            filters.Add(branchFilter);
        }

        if (adjustmentType.HasValue)
        {
            filters.Add(Builders<InventoryAdjustmentDocument>.Filter.Eq(a => a.AdjustmentType, adjustmentType.Value));
        }

        if (status.HasValue)
        {
            filters.Add(Builders<InventoryAdjustmentDocument>.Filter.Eq(a => a.Status, status.Value));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<InventoryAdjustmentDocument>.Filter.Gte(a => a.AdjustmentDateUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<InventoryAdjustmentDocument>.Filter.Lte(a => a.AdjustmentDateUtc, to.Value));
        }

        return filters.Count > 0
            ? Builders<InventoryAdjustmentDocument>.Filter.And(filters)
            : FilterDefinition<InventoryAdjustmentDocument>.Empty;
    }
}
