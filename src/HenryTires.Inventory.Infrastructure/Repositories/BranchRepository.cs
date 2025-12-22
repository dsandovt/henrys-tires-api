using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class BranchRepository : CrudRepository<Branch>, IBranchRepository
{
    public BranchRepository(IMongoClient client)
        : base(client, "Inventory", "Branch") { }

    public async Task<Branch?> GetByCodeAsync(string code)
    {
        return await _collection.Find(b => b.Code == code).FirstOrDefaultAsync();
    }
}
