using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class RoleRepository : CrudRepository<RoleDocument>, IRoleRepository
{
    public RoleRepository(IMongoClient client)
        : base(client, "Inventory", "Role") { }

    public async Task<Role?> GetByCodeAsync(string code)
    {
        var document = await _collection.Find(r => r.Code == code).FirstOrDefaultAsync();
        return document == null ? null : RoleDocumentMapper.ToEntity(document);
    }

    public new async Task<Role?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : RoleDocumentMapper.ToEntity(document);
    }

    public new async Task<IEnumerable<Role>> GetAllAsync()
    {
        var documents = await base.GetAllAsync();
        return documents.Select(RoleDocumentMapper.ToEntity);
    }

    public async Task<IEnumerable<Role>> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? FilterDefinition<RoleDocument>.Empty
            : Builders<RoleDocument>.Filter.Or(
                Builders<RoleDocument>.Filter.Regex(r => r.Code, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<RoleDocument>.Filter.Regex(r => r.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

        var documents = await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(RoleDocumentMapper.ToEntity);
    }

    public async Task<int> CountAsync(string? searchTerm)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? FilterDefinition<RoleDocument>.Empty
            : Builders<RoleDocument>.Filter.Or(
                Builders<RoleDocument>.Filter.Regex(r => r.Code, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<RoleDocument>.Filter.Regex(r => r.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        var document = RoleDocumentMapper.ToDocument(role);
        var result = await UpsertAsync(null, document);
        return RoleDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(Role role)
    {
        var document = RoleDocumentMapper.ToDocument(role);
        await UpsertAsync(role.Id, document);
    }

    public async Task DeleteAsync(string id)
    {
        await DeleteByIdAsync(id);
    }
}
