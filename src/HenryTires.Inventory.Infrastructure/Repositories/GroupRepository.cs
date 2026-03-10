using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class GroupRepository : CrudRepository<GroupDocument>, IGroupRepository
{
    public GroupRepository(IMongoClient client)
        : base(client, "Inventory", "Group") { }

    public async Task<Group?> GetByCodeAsync(string code)
    {
        var document = await _collection.Find(g => g.Code == code).FirstOrDefaultAsync();
        return document == null ? null : GroupDocumentMapper.ToEntity(document);
    }

    public new async Task<Group?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : GroupDocumentMapper.ToEntity(document);
    }

    public new async Task<IEnumerable<Group>> GetAllAsync()
    {
        var documents = await base.GetAllAsync();
        return documents.Select(GroupDocumentMapper.ToEntity);
    }

    public async Task<IEnumerable<Group>> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? FilterDefinition<GroupDocument>.Empty
            : Builders<GroupDocument>.Filter.Or(
                Builders<GroupDocument>.Filter.Regex(g => g.Code, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<GroupDocument>.Filter.Regex(g => g.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

        var documents = await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(GroupDocumentMapper.ToEntity);
    }

    public async Task<int> CountAsync(string? searchTerm)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? FilterDefinition<GroupDocument>.Empty
            : Builders<GroupDocument>.Filter.Or(
                Builders<GroupDocument>.Filter.Regex(g => g.Code, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<GroupDocument>.Filter.Regex(g => g.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<Group> CreateAsync(Group group)
    {
        var document = GroupDocumentMapper.ToDocument(group);
        var result = await UpsertAsync(null, document);
        return GroupDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(Group group)
    {
        var document = GroupDocumentMapper.ToDocument(group);
        await UpsertAsync(group.Id, document);
    }

    public async Task DeleteAsync(string id)
    {
        await DeleteByIdAsync(id);
    }

    public async Task<IEnumerable<Group>> GetByIdsAsync(List<string> ids)
    {
        var documents = await base.GetByIdsAsync(ids);
        return documents.Select(GroupDocumentMapper.ToEntity);
    }
}
