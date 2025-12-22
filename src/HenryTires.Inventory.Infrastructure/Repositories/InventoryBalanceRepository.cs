using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class InventoryBalanceRepository : CrudRepository<InventoryBalance>, IInventoryBalanceRepository
{
    public InventoryBalanceRepository(IMongoClient client)
        : base(client, "Inventory", "InventoryBalance") { }

    public async Task<InventoryBalance?> GetByKeyAsync(
        string branchId,
        string itemId,
        ItemCondition condition
    )
    {
        var filter = Builders<InventoryBalance>.Filter.And(
            Builders<InventoryBalance>.Filter.Eq(b => b.BranchId, branchId),
            Builders<InventoryBalance>.Filter.Eq(b => b.ItemId, itemId),
            Builders<InventoryBalance>.Filter.Eq(b => b.Condition, condition)
        );

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<InventoryBalance>> GetByBranchAsync(
        string branchId,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    )
    {
        var filters = new List<FilterDefinition<InventoryBalance>>
        {
            Builders<InventoryBalance>.Filter.Eq(b => b.BranchId, branchId),
        };

        if (condition.HasValue)
        {
            filters.Add(Builders<InventoryBalance>.Filter.Eq(b => b.Condition, condition.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventoryBalance>.Filter.Regex(
                    b => b.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        var filter = Builders<InventoryBalance>.Filter.And(filters);

        return await _collection
            .Find(filter)
            .SortBy(b => b.ItemCode)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountByBranchAsync(
        string branchId,
        string? search,
        ItemCondition? condition
    )
    {
        var filters = new List<FilterDefinition<InventoryBalance>>
        {
            Builders<InventoryBalance>.Filter.Eq(b => b.BranchId, branchId),
        };

        if (condition.HasValue)
        {
            filters.Add(Builders<InventoryBalance>.Filter.Eq(b => b.Condition, condition.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventoryBalance>.Filter.Regex(
                    b => b.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        var filter = Builders<InventoryBalance>.Filter.And(filters);

        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<int> GetTotalQuantityByBranchAsync(string branchId)
    {
        var filter = Builders<InventoryBalance>.Filter.Eq(b => b.BranchId, branchId);
        var balances = await _collection.Find(filter).ToListAsync();
        return balances.Sum(b => b.QuantityOnHand);
    }

    public async Task<IEnumerable<InventoryBalance>> GetAllBranchesStockAsync(
        string? search,
        ItemCondition? condition
    )
    {
        var filters = new List<FilterDefinition<InventoryBalance>>();

        if (condition.HasValue)
        {
            filters.Add(Builders<InventoryBalance>.Filter.Eq(b => b.Condition, condition.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventoryBalance>.Filter.Regex(
                    b => b.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventoryBalance>.Filter.And(filters)
                : FilterDefinition<InventoryBalance>.Empty;

        return await _collection.Find(filter).SortBy(b => b.ItemCode).ToListAsync();
    }

    public async Task UpsertAsync(InventoryBalance balance, IClientSessionHandle? session = null)
    {
        await base.UpsertAsync(balance.Id, balance, session);
    }
}
