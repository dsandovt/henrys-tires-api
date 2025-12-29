using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class UserRepository : CrudRepository<UserDocument>, IUserRepository
{
    public UserRepository(IMongoClient client)
        : base(client, "Inventory", "Users") { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var document = await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();
        return document == null ? null : UserDocumentMapper.ToEntity(document);
    }

    public async Task<IEnumerable<User>> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? FilterDefinition<UserDocument>.Empty
            : Builders<UserDocument>.Filter.Regex(
                u => u.Username,
                new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")
            );

        var documents = await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(UserDocumentMapper.ToEntity);
    }

    public async Task<int> CountAsync(string? searchTerm)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? FilterDefinition<UserDocument>.Empty
            : Builders<UserDocument>.Filter.Regex(
                u => u.Username,
                new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")
            );

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public new async Task<User?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : UserDocumentMapper.ToEntity(document);
    }

    public async Task<User> CreateAsync(User user)
    {
        var document = UserDocumentMapper.ToDocument(user);
        var result = await UpsertAsync(null, document);
        return UserDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(User user)
    {
        var document = UserDocumentMapper.ToDocument(user);
        await UpsertAsync(user.Id, document);
    }

    public async Task DeleteAsync(string id)
    {
        await DeleteByIdAsync(id);
    }

    public new async Task<IEnumerable<User>> GetAllAsync()
    {
        var documents = await base.GetAllAsync();
        return documents.Select(UserDocumentMapper.ToEntity);
    }
}
