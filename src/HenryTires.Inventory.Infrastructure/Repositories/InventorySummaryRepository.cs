using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class InventorySummaryRepository : CrudRepository<InventorySummary>, IInventorySummaryRepository
{
    public InventorySummaryRepository(IMongoClient client)
        : base(client, "Inventory", "InventorySummary")
    {
        var indexKeys = Builders<InventorySummary>
            .IndexKeys.Ascending(s => s.BranchCode)
            .Ascending(s => s.ItemCode);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<InventorySummary>(indexKeys, indexOptions);
        _collection.Indexes.CreateOneAsync(indexModel);

        var branchIndexKeys = Builders<InventorySummary>.IndexKeys.Ascending(s => s.BranchCode);
        var branchIndexModel = new CreateIndexModel<InventorySummary>(branchIndexKeys);
        _collection.Indexes.CreateOneAsync(branchIndexModel);
    }

    public async Task<InventorySummary?> GetByKeyAsync(
        string branchCode,
        string itemCode,
        IClientSessionHandle? session = null
    )
    {
        var filter = Builders<InventorySummary>.Filter.And(
            Builders<InventorySummary>.Filter.Eq(s => s.BranchCode, branchCode),
            Builders<InventorySummary>.Filter.Eq(s => s.ItemCode, itemCode)
        );

        if (session != null)
            return await _collection.Find(session, filter).FirstOrDefaultAsync();
        else
            return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<InventorySummary>> GetByBranchAsync(
        string? branchCode,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    )
    {
        var filters = new List<FilterDefinition<InventorySummary>>();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            filters.Add(Builders<InventorySummary>.Filter.Eq(s => s.BranchCode, branchCode));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventorySummary>.Filter.Regex(
                    s => s.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        if (condition.HasValue)
        {
            filters.Add(
                Builders<InventorySummary>.Filter.ElemMatch(
                    s => s.Entries,
                    e => e.Condition == condition.Value
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventorySummary>.Filter.And(filters)
                : Builders<InventorySummary>.Filter.Empty;

        return await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountByBranchAsync(
        string? branchCode,
        string? search,
        ItemCondition? condition
    )
    {
        var filters = new List<FilterDefinition<InventorySummary>>();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            filters.Add(Builders<InventorySummary>.Filter.Eq(s => s.BranchCode, branchCode));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventorySummary>.Filter.Regex(
                    s => s.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        if (condition.HasValue)
        {
            filters.Add(
                Builders<InventorySummary>.Filter.ElemMatch(
                    s => s.Entries,
                    e => e.Condition == condition.Value
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventorySummary>.Filter.And(filters)
                : Builders<InventorySummary>.Filter.Empty;

        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<int> GetTotalQuantityByBranchAsync(string branchCode)
    {
        var summaries = await _collection.Find(s => s.BranchCode == branchCode).ToListAsync();
        return summaries.Sum(s => s.OnHandTotal);
    }

    public async Task UpsertAsync(InventorySummary summary, IClientSessionHandle? session = null)
    {
        await base.UpsertAsync(summary.Id, summary, session);
    }

    public async Task UpsertWithVersionCheckAsync(
        InventorySummary summary,
        IClientSessionHandle session
    )
    {
        var filter = Builders<InventorySummary>.Filter.And(
            Builders<InventorySummary>.Filter.Eq(s => s.BranchCode, summary.BranchCode),
            Builders<InventorySummary>.Filter.Eq(s => s.ItemCode, summary.ItemCode),
            Builders<InventorySummary>.Filter.Eq(s => s.Version, summary.Version - 1) // Check previous version
        );

        var result = await _collection.ReplaceOneAsync(
            session,
            filter,
            summary,
            new ReplaceOptions { IsUpsert = true }
        );

        if (result.ModifiedCount == 0 && result.UpsertedId == null)
        {
            throw new ConcurrencyException(
                $"Inventory summary for {summary.BranchCode}/{summary.ItemCode} was modified by another transaction. Please retry."
            );
        }
    }
}
