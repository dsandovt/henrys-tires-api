using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using HenryTires.Inventory.Infrastructure.Adapters.Transactions;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class InventoryTransactionRepository : CrudRepository<InventoryTransactionDocument>, IInventoryTransactionRepository
{
    public InventoryTransactionRepository(IMongoClient client)
        : base(client, "Inventory", "InventoryTransaction") { }

    public async Task<InventoryTransaction?> GetByTransactionNumberAsync(string transactionNumber)
    {
        var document = await _collection
            .Find(t => t.TransactionNumber == transactionNumber)
            .FirstOrDefaultAsync();

        return document == null ? null : InventoryTransactionDocumentMapper.ToEntity(document);
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
        var filters = new List<FilterDefinition<InventoryTransactionDocument>>();

        if (!string.IsNullOrEmpty(branchCode))
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Eq(t => t.BranchCode, branchCode));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Gte(t => t.CreatedAtUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Lte(t => t.CreatedAtUtc, to.Value));
        }

        if (type.HasValue)
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Eq(t => t.Type, type.Value));
        }

        if (status.HasValue)
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Eq(t => t.Status, status.Value));
        }

        if (!string.IsNullOrEmpty(itemCode))
        {
            filters.Add(
                Builders<InventoryTransactionDocument>.Filter.ElemMatch(
                    t => t.Lines,
                    l => l.ItemCode == itemCode
                )
            );
        }

        if (condition.HasValue)
        {
            filters.Add(
                Builders<InventoryTransactionDocument>.Filter.ElemMatch(
                    t => t.Lines,
                    l => l.Condition == condition.Value
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventoryTransactionDocument>.Filter.And(filters)
                : FilterDefinition<InventoryTransactionDocument>.Empty;

        var documents = await _collection
            .Find(filter)
            .SortByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(InventoryTransactionDocumentMapper.ToEntity);
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
        var filters = new List<FilterDefinition<InventoryTransactionDocument>>();

        if (!string.IsNullOrEmpty(branchCode))
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Eq(t => t.BranchCode, branchCode));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Gte(t => t.CreatedAtUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Lte(t => t.CreatedAtUtc, to.Value));
        }

        if (type.HasValue)
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Eq(t => t.Type, type.Value));
        }

        if (status.HasValue)
        {
            filters.Add(Builders<InventoryTransactionDocument>.Filter.Eq(t => t.Status, status.Value));
        }

        if (!string.IsNullOrEmpty(itemCode))
        {
            filters.Add(
                Builders<InventoryTransactionDocument>.Filter.ElemMatch(
                    t => t.Lines,
                    l => l.ItemCode == itemCode
                )
            );
        }

        if (condition.HasValue)
        {
            filters.Add(
                Builders<InventoryTransactionDocument>.Filter.ElemMatch(
                    t => t.Lines,
                    l => l.Condition == condition.Value
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventoryTransactionDocument>.Filter.And(filters)
                : FilterDefinition<InventoryTransactionDocument>.Empty;

        return await _collection.CountDocumentsAsync(filter);
    }

    public new async Task<InventoryTransaction?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : InventoryTransactionDocumentMapper.ToEntity(document);
    }

    public async Task<InventoryTransaction> CreateAsync(InventoryTransaction transaction)
    {
        var document = InventoryTransactionDocumentMapper.ToDocument(transaction);
        var result = await UpsertAsync(null, document);
        return InventoryTransactionDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(InventoryTransaction transaction, ITransactionScope? transactionScope = null)
    {
        var document = InventoryTransactionDocumentMapper.ToDocument(transaction);
        var session = transactionScope.ToMongoSession();
        await UpsertAsync(transaction.Id, document, session);
    }
}
