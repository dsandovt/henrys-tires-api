using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class SaleRepository : CrudRepository<SaleDocument>, ISaleRepository
{
    public SaleRepository(IMongoClient client)
        : base(client, "Inventory", "Sale") { }

    public async Task<IEnumerable<Sale>> GetByBranchAndDateRangeAsync(
        string branchId,
        DateTime from,
        DateTime to
    )
    {
        var documents = await _collection
            .Find(s => s.BranchId == branchId && s.SaleDateUtc >= from && s.SaleDateUtc <= to)
            .SortByDescending(s => s.SaleDateUtc)
            .ToListAsync();

        return documents.Select(SaleDocumentMapper.ToEntity);
    }

    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var documents = await _collection
            .Find(s => s.SaleDateUtc >= from && s.SaleDateUtc <= to)
            .SortByDescending(s => s.SaleDateUtc)
            .ToListAsync();

        return documents.Select(SaleDocumentMapper.ToEntity);
    }

    public async Task<IEnumerable<Sale>> SearchAsync(
        string? branchId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    )
    {
        var filters = new List<FilterDefinition<SaleDocument>>();

        if (!string.IsNullOrEmpty(branchId))
        {
            filters.Add(Builders<SaleDocument>.Filter.Eq(s => s.BranchId, branchId));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<SaleDocument>.Filter.Gte(s => s.SaleDateUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<SaleDocument>.Filter.Lte(s => s.SaleDateUtc, to.Value));
        }

        var filter =
            filters.Count > 0 ? Builders<SaleDocument>.Filter.And(filters) : FilterDefinition<SaleDocument>.Empty;

        var documents = await _collection
            .Find(filter)
            .SortByDescending(s => s.SaleDateUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(SaleDocumentMapper.ToEntity);
    }

    public async Task<int> CountAsync(string? branchId, DateTime? from, DateTime? to)
    {
        var filters = new List<FilterDefinition<SaleDocument>>();

        if (!string.IsNullOrEmpty(branchId))
        {
            filters.Add(Builders<SaleDocument>.Filter.Eq(s => s.BranchId, branchId));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<SaleDocument>.Filter.Gte(s => s.SaleDateUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<SaleDocument>.Filter.Lte(s => s.SaleDateUtc, to.Value));
        }

        var filter =
            filters.Count > 0 ? Builders<SaleDocument>.Filter.And(filters) : FilterDefinition<SaleDocument>.Empty;

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<Sale> CreateAsync(Sale sale)
    {
        var document = SaleDocumentMapper.ToDocument(sale);
        var result = await UpsertAsync(null, document);
        return SaleDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(Sale sale)
    {
        var document = SaleDocumentMapper.ToDocument(sale);
        await UpsertAsync(sale.Id, document);
    }

    public new async Task<Sale?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : SaleDocumentMapper.ToEntity(document);
    }
}
