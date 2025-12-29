using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using HenryTires.Inventory.Infrastructure.Adapters.Transactions;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class InventorySummaryRepository : CrudRepository<InventorySummaryDocument>, IInventorySummaryRepository
{
    public InventorySummaryRepository(IMongoClient client)
        : base(client, "Inventory", "InventorySummary")
    {
        var indexKeys = Builders<InventorySummaryDocument>
            .IndexKeys.Ascending(s => s.BranchCode)
            .Ascending(s => s.ItemCode);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<InventorySummaryDocument>(indexKeys, indexOptions);
        _collection.Indexes.CreateOneAsync(indexModel);

        var branchIndexKeys = Builders<InventorySummaryDocument>.IndexKeys.Ascending(s => s.BranchCode);
        var branchIndexModel = new CreateIndexModel<InventorySummaryDocument>(branchIndexKeys);
        _collection.Indexes.CreateOneAsync(branchIndexModel);
    }

    public async Task<InventorySummary?> GetByKeyAsync(
        string branchCode,
        string itemCode,
        ITransactionScope? transactionScope = null
    )
    {
        var session = transactionScope.ToMongoSession();

        var filter = Builders<InventorySummaryDocument>.Filter.And(
            Builders<InventorySummaryDocument>.Filter.Eq(s => s.BranchCode, branchCode),
            Builders<InventorySummaryDocument>.Filter.Eq(s => s.ItemCode, itemCode)
        );

        InventorySummaryDocument? document;
        if (session != null)
            document = await _collection.Find(session, filter).FirstOrDefaultAsync();
        else
            document = await _collection.Find(filter).FirstOrDefaultAsync();

        return document == null ? null : InventorySummaryDocumentMapper.ToEntity(document);
    }

    public async Task<IEnumerable<InventorySummary>> GetByBranchAsync(
        string? branchCode,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    )
    {
        var filters = new List<FilterDefinition<InventorySummaryDocument>>();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            filters.Add(Builders<InventorySummaryDocument>.Filter.Eq(s => s.BranchCode, branchCode));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventorySummaryDocument>.Filter.Regex(
                    s => s.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        if (condition.HasValue)
        {
            filters.Add(
                Builders<InventorySummaryDocument>.Filter.ElemMatch(
                    s => s.Entries,
                    e => e.Condition == condition.Value
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventorySummaryDocument>.Filter.And(filters)
                : Builders<InventorySummaryDocument>.Filter.Empty;

        var documents = await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(InventorySummaryDocumentMapper.ToEntity);
    }

    public async Task<long> CountByBranchAsync(
        string? branchCode,
        string? search,
        ItemCondition? condition
    )
    {
        var filters = new List<FilterDefinition<InventorySummaryDocument>>();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            filters.Add(Builders<InventorySummaryDocument>.Filter.Eq(s => s.BranchCode, branchCode));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                Builders<InventorySummaryDocument>.Filter.Regex(
                    s => s.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
        }

        if (condition.HasValue)
        {
            filters.Add(
                Builders<InventorySummaryDocument>.Filter.ElemMatch(
                    s => s.Entries,
                    e => e.Condition == condition.Value
                )
            );
        }

        var filter =
            filters.Count > 0
                ? Builders<InventorySummaryDocument>.Filter.And(filters)
                : Builders<InventorySummaryDocument>.Filter.Empty;

        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<int> GetTotalQuantityByBranchAsync(string branchCode)
    {
        var documents = await _collection.Find(s => s.BranchCode == branchCode).ToListAsync();
        return documents.Sum(s => s.OnHandTotal);
    }

    public async Task UpsertAsync(InventorySummary summary, ITransactionScope? transactionScope = null)
    {
        var document = InventorySummaryDocumentMapper.ToDocument(summary);
        var session = transactionScope.ToMongoSession();
        await base.UpsertAsync(summary.Id, document, session);
    }

    public async Task UpsertWithVersionCheckAsync(
        InventorySummary summary,
        ITransactionScope transactionScope
    )
    {
        var document = InventorySummaryDocumentMapper.ToDocument(summary);
        var session = transactionScope.ToMongoSession();

        if (session == null)
            throw new InvalidOperationException("UpsertWithVersionCheckAsync requires a transaction scope.");

        var filter = Builders<InventorySummaryDocument>.Filter.And(
            Builders<InventorySummaryDocument>.Filter.Eq(s => s.BranchCode, summary.BranchCode),
            Builders<InventorySummaryDocument>.Filter.Eq(s => s.ItemCode, summary.ItemCode),
            Builders<InventorySummaryDocument>.Filter.Eq(s => s.Version, summary.Version - 1) // Check previous version
        );

        var result = await _collection.ReplaceOneAsync(
            session,
            filter,
            document,
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
