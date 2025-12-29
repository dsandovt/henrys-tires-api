using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class InventoryBalanceRepository : CrudRepository<InventoryBalanceDocument>, IInventoryBalanceRepository
{
    public InventoryBalanceRepository(IMongoClient client)
        : base(client, "Inventory", "InventoryBalance") { }

    public async Task<InventoryBalance?> GetByKeyAsync(
        string branchId,
        string itemId,
        ItemCondition condition
    )
    {
        var filter = Builders<InventoryBalanceDocument>.Filter.And(
            Builders<InventoryBalanceDocument>.Filter.Eq(b => b.BranchId, branchId),
            Builders<InventoryBalanceDocument>.Filter.Eq(b => b.ItemId, itemId),
            Builders<InventoryBalanceDocument>.Filter.Eq(b => b.Condition, condition)
        );

        var document = await _collection.Find(filter).FirstOrDefaultAsync();
        return document == null ? null : InventoryBalanceDocumentMapper.ToEntity(document);
    }

    public async Task<IEnumerable<InventoryBalance>> GetByBranchAsync(
        string branchId,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    )
    {
        var filters = new List<FilterDefinition<InventoryBalanceDocument>>
        {
            Builders<InventoryBalanceDocument>.Filter.Eq(b => b.BranchId, branchId),
        };

        if (condition.HasValue)
        {
            filters.Add(Builders<InventoryBalanceDocument>.Filter.Eq(b => b.Condition, condition.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventoryBalanceDocument>.Filter.Regex(
                    b => b.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        var filter = Builders<InventoryBalanceDocument>.Filter.And(filters);

        var documents = await _collection
            .Find(filter)
            .SortBy(b => b.ItemCode)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(InventoryBalanceDocumentMapper.ToEntity);
    }

    public async Task<long> CountByBranchAsync(
        string branchId,
        string? search,
        ItemCondition? condition
    )
    {
        var filters = new List<FilterDefinition<InventoryBalanceDocument>>
        {
            Builders<InventoryBalanceDocument>.Filter.Eq(b => b.BranchId, branchId),
        };

        if (condition.HasValue)
        {
            filters.Add(Builders<InventoryBalanceDocument>.Filter.Eq(b => b.Condition, condition.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventoryBalanceDocument>.Filter.Regex(
                    b => b.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        var filter = Builders<InventoryBalanceDocument>.Filter.And(filters);

        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<int> GetTotalQuantityByBranchAsync(string branchId)
    {
        var filter = Builders<InventoryBalanceDocument>.Filter.Eq(b => b.BranchId, branchId);
        var documents = await _collection.Find(filter).ToListAsync();
        return documents.Sum(b => b.QuantityOnHand);
    }

    public async Task<IEnumerable<InventoryBalance>> GetAllBranchesStockAsync(
        string? search,
        ItemCondition? condition
    )
    {
        var filters = new List<FilterDefinition<InventoryBalanceDocument>>();

        if (condition.HasValue)
        {
            filters.Add(Builders<InventoryBalanceDocument>.Filter.Eq(b => b.Condition, condition.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventoryBalanceDocument>.Filter.Regex(
                    b => b.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventoryBalanceDocument>.Filter.And(filters)
                : FilterDefinition<InventoryBalanceDocument>.Empty;

        var documents = await _collection.Find(filter).SortBy(b => b.ItemCode).ToListAsync();
        return documents.Select(InventoryBalanceDocumentMapper.ToEntity);
    }

    public async Task UpsertAsync(InventoryBalance balance, IClientSessionHandle? session = null)
    {
        var document = InventoryBalanceDocumentMapper.ToDocument(balance);
        await base.UpsertAsync(balance.Id, document, session);
    }
}
