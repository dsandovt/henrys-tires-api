using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class ItemRepository : CrudRepository<Item>, IItemRepository
{
    public ItemRepository(IMongoClient client)
        : base(client, "Inventory", "Item")
    {
        var indexKeys = Builders<Item>.IndexKeys.Ascending(i => i.ItemCode);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<Item>(indexKeys, indexOptions);
        _collection.Indexes.CreateOneAsync(indexModel);
    }

    public async Task<Item?> GetByItemCodeAsync(string itemCode)
    {
        return await _collection
            .Find(i => i.ItemCode == itemCode && !i.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Item>> SearchAsync(
        string? search,
        Classification? classification,
        int page,
        int pageSize
    )
    {
        var filter = Builders<Item>.Filter.Eq(i => i.IsDeleted, false);

        if (classification.HasValue)
        {
            filter = Builders<Item>.Filter.And(
                filter,
                Builders<Item>.Filter.Eq(i => i.Classification, classification.Value)
            );
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchFilter = Builders<Item>.Filter.Or(
                Builders<Item>.Filter.Regex(
                    i => i.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                ),
                Builders<Item>.Filter.Regex(
                    i => i.Description,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
            filter = Builders<Item>.Filter.And(filter, searchFilter);
        }

        return await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? search, Classification? classification)
    {
        var filter = Builders<Item>.Filter.Eq(i => i.IsDeleted, false);

        if (classification.HasValue)
        {
            filter = Builders<Item>.Filter.And(
                filter,
                Builders<Item>.Filter.Eq(i => i.Classification, classification.Value)
            );
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchFilter = Builders<Item>.Filter.Or(
                Builders<Item>.Filter.Regex(
                    i => i.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                ),
                Builders<Item>.Filter.Regex(
                    i => i.Description,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
            filter = Builders<Item>.Filter.And(filter, searchFilter);
        }

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public new async Task<Item?> GetByIdAsync(string id)
    {
        return await _collection.Find(i => i.Id == id && !i.IsDeleted).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Item>> GetAllAsync(Classification? classification = null)
    {
        var filter = Builders<Item>.Filter.Eq(i => i.IsDeleted, false);

        if (classification.HasValue)
        {
            filter = Builders<Item>.Filter.And(
                filter,
                Builders<Item>.Filter.Eq(i => i.Classification, classification.Value)
            );
        }

        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Item> CreateAsync(Item item)
    {
        return await UpsertAsync(null, item);
    }

    public async Task UpdateAsync(Item item)
    {
        await UpsertAsync(item.Id, item);
    }

    public async Task DeleteAsync(string id, string deletedBy, DateTime deletedAtUtc)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            item.IsDeleted = true;
            item.DeletedBy = deletedBy;
            item.DeletedAtUtc = deletedAtUtc;
            await UpsertAsync(item.Id, item);
        }
    }
}
