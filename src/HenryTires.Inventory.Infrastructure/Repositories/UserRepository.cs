using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class UserRepository : CrudRepository<User>, IUserRepository
{
    public UserRepository(IMongoClient client)
        : base(client, "Inventory", "Users") { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? FilterDefinition<User>.Empty
            : Builders<User>.Filter.Regex(
                u => u.Username,
                new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")
            );

        return await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? searchTerm)
    {
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? FilterDefinition<User>.Empty
            : Builders<User>.Filter.Regex(
                u => u.Username,
                new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")
            );

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<User> CreateAsync(User user)
    {
        return await UpsertAsync(null, user);
    }

    public async Task UpdateAsync(User user)
    {
        await UpsertAsync(user.Id, user);
    }

    public async Task DeleteAsync(string id)
    {
        await DeleteByIdAsync(id);
    }
}
