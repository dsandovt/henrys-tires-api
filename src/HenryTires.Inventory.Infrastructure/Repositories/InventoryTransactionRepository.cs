using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class InventoryTransactionRepository : CrudRepository<InventoryTransaction>, IInventoryTransactionRepository
{
    public InventoryTransactionRepository(IMongoClient client)
        : base(client, "Inventory", "InventoryTransaction") { }

    public async Task<InventoryTransaction?> GetByTransactionNumberAsync(string transactionNumber)
    {
        return await _collection
            .Find(t => t.TransactionNumber == transactionNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> SearchAsync(
        string? branchCode,
        DateTime? from,
        DateTime? to,
        TransactionType? type,
        TransactionStatus? status,
        string? itemCode,
        ItemCondition? condition,
        int page,
        int pageSize
    )
    {
        var filters = new List<FilterDefinition<InventoryTransaction>>();

        if (!string.IsNullOrEmpty(branchCode))
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Eq(t => t.BranchCode, branchCode));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Gte(t => t.CreatedAtUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Lte(t => t.CreatedAtUtc, to.Value));
        }

        if (type.HasValue)
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Eq(t => t.Type, type.Value));
        }

        if (status.HasValue)
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Eq(t => t.Status, status.Value));
        }

        if (!string.IsNullOrEmpty(itemCode))
        {
            filters.Add(
                Builders<InventoryTransaction>.Filter.ElemMatch(
                    t => t.Lines,
                    l => l.ItemCode == itemCode
                )
            );
        }

        if (condition.HasValue)
        {
            filters.Add(
                Builders<InventoryTransaction>.Filter.ElemMatch(
                    t => t.Lines,
                    l => l.Condition == condition.Value
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventoryTransaction>.Filter.And(filters)
                : FilterDefinition<InventoryTransaction>.Empty;

        return await _collection
            .Find(filter)
            .SortByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountAsync(
        string? branchCode,
        DateTime? from,
        DateTime? to,
        TransactionType? type,
        TransactionStatus? status,
        string? itemCode,
        ItemCondition? condition
    )
    {
        var filters = new List<FilterDefinition<InventoryTransaction>>();

        if (!string.IsNullOrEmpty(branchCode))
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Eq(t => t.BranchCode, branchCode));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Gte(t => t.CreatedAtUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Lte(t => t.CreatedAtUtc, to.Value));
        }

        if (type.HasValue)
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Eq(t => t.Type, type.Value));
        }

        if (status.HasValue)
        {
            filters.Add(Builders<InventoryTransaction>.Filter.Eq(t => t.Status, status.Value));
        }

        if (!string.IsNullOrEmpty(itemCode))
        {
            filters.Add(
                Builders<InventoryTransaction>.Filter.ElemMatch(
                    t => t.Lines,
                    l => l.ItemCode == itemCode
                )
            );
        }

        if (condition.HasValue)
        {
            filters.Add(
                Builders<InventoryTransaction>.Filter.ElemMatch(
                    t => t.Lines,
                    l => l.Condition == condition.Value
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventoryTransaction>.Filter.And(filters)
                : FilterDefinition<InventoryTransaction>.Empty;

        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<InventoryTransaction> CreateAsync(InventoryTransaction transaction)
    {
        return await UpsertAsync(null, transaction);
    }

    public async Task UpdateAsync(InventoryTransaction transaction, IClientSessionHandle? session = null)
    {
        await UpsertAsync(transaction.Id, transaction, session);
    }
}
