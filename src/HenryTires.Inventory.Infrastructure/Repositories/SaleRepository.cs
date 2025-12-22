using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class SaleRepository : CrudRepository<Sale>, ISaleRepository
{
    public SaleRepository(IMongoClient client)
        : base(client, "Inventory", "Sales") { }

    public async Task<IEnumerable<Sale>> GetByBranchAndDateRangeAsync(
        string branchId,
        DateTime from,
        DateTime to
    )
    {
        return await _collection
            .Find(s => s.BranchId == branchId && s.SaleDateUtc >= from && s.SaleDateUtc <= to)
            .SortByDescending(s => s.SaleDateUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _collection
            .Find(s => s.SaleDateUtc >= from && s.SaleDateUtc <= to)
            .SortByDescending(s => s.SaleDateUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> SearchAsync(
        string? branchId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    )
    {
        var filters = new List<FilterDefinition<Sale>>();

        if (!string.IsNullOrEmpty(branchId))
        {
            filters.Add(Builders<Sale>.Filter.Eq(s => s.BranchId, branchId));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<Sale>.Filter.Gte(s => s.SaleDateUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<Sale>.Filter.Lte(s => s.SaleDateUtc, to.Value));
        }

        var filter =
            filters.Count > 0 ? Builders<Sale>.Filter.And(filters) : FilterDefinition<Sale>.Empty;

        return await _collection
            .Find(filter)
            .SortByDescending(s => s.SaleDateUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? branchId, DateTime? from, DateTime? to)
    {
        var filters = new List<FilterDefinition<Sale>>();

        if (!string.IsNullOrEmpty(branchId))
        {
            filters.Add(Builders<Sale>.Filter.Eq(s => s.BranchId, branchId));
        }

        if (from.HasValue)
        {
            filters.Add(Builders<Sale>.Filter.Gte(s => s.SaleDateUtc, from.Value));
        }

        if (to.HasValue)
        {
            filters.Add(Builders<Sale>.Filter.Lte(s => s.SaleDateUtc, to.Value));
        }

        var filter =
            filters.Count > 0 ? Builders<Sale>.Filter.And(filters) : FilterDefinition<Sale>.Empty;

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<Sale> CreateAsync(Sale sale)
    {
        return await UpsertAsync(null, sale);
    }

    public async Task UpdateAsync(Sale sale)
    {
        await UpsertAsync(sale.Id, sale);
    }
}
