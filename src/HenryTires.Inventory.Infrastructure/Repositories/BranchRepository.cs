using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class BranchRepository : CrudRepository<BranchDocument>, IBranchRepository
{
    public BranchRepository(IMongoClient client)
        : base(client, "Inventory", "Branch") { }

    public async Task<Branch?> GetByCodeAsync(string code)
    {
        var document = await _collection.Find(b => b.Code == code).FirstOrDefaultAsync();
        return document == null ? null : BranchDocumentMapper.ToEntity(document);
    }

    public new async Task<Branch?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : BranchDocumentMapper.ToEntity(document);
    }

    public new async Task<IEnumerable<Branch>> GetAllAsync()
    {
        var documents = await base.GetAllAsync();
        return documents.Select(BranchDocumentMapper.ToEntity);
    }
}
