using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class ItemRepository : CrudRepository<ItemDocument>, IItemRepository
{
    public ItemRepository(IMongoClient client)
        : base(client, "Inventory", "Item")
    {
        var indexKeys = Builders<ItemDocument>.IndexKeys.Ascending(i => i.ItemCode);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<ItemDocument>(indexKeys, indexOptions);
        _collection.Indexes.CreateOneAsync(indexModel);
    }

    public async Task<Item?> GetByItemCodeAsync(string itemCode)
    {
        var document = await _collection
            .Find(i => i.ItemCode == itemCode && !i.IsDeleted)
            .FirstOrDefaultAsync();

        return document == null ? null : ItemDocumentMapper.ToEntity(document);
    }

    public async Task<IEnumerable<Item>> SearchAsync(
        string? search,
        Classification? classification,
        int page,
        int pageSize
    )
    {
        var filter = Builders<ItemDocument>.Filter.Eq(i => i.IsDeleted, false);

        if (classification.HasValue)
        {
            filter = Builders<ItemDocument>.Filter.And(
                filter,
                Builders<ItemDocument>.Filter.Eq(i => i.Classification, classification.Value)
            );
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchFilter = Builders<ItemDocument>.Filter.Or(
                Builders<ItemDocument>.Filter.Regex(
                    i => i.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                ),
                Builders<ItemDocument>.Filter.Regex(
                    i => i.Description,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
            filter = Builders<ItemDocument>.Filter.And(filter, searchFilter);
        }

        var documents = await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return documents.Select(ItemDocumentMapper.ToEntity);
    }

    public async Task<int> CountAsync(string? search, Classification? classification)
    {
        var filter = Builders<ItemDocument>.Filter.Eq(i => i.IsDeleted, false);

        if (classification.HasValue)
        {
            filter = Builders<ItemDocument>.Filter.And(
                filter,
                Builders<ItemDocument>.Filter.Eq(i => i.Classification, classification.Value)
            );
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchFilter = Builders<ItemDocument>.Filter.Or(
                Builders<ItemDocument>.Filter.Regex(
                    i => i.ItemCode,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                ),
                Builders<ItemDocument>.Filter.Regex(
                    i => i.Description,
                    new MongoDB.Bson.BsonRegularExpression(search, "i")
                )
            );
            filter = Builders<ItemDocument>.Filter.And(filter, searchFilter);
        }

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public new async Task<Item?> GetByIdAsync(string id)
    {
        var document = await _collection.Find(i => i.Id == id && !i.IsDeleted).FirstOrDefaultAsync();
        return document == null ? null : ItemDocumentMapper.ToEntity(document);
    }

    public async Task<IEnumerable<Item>> GetAllAsync(Classification? classification = null)
    {
        var filter = Builders<ItemDocument>.Filter.Eq(i => i.IsDeleted, false);

        if (classification.HasValue)
        {
            filter = Builders<ItemDocument>.Filter.And(
                filter,
                Builders<ItemDocument>.Filter.Eq(i => i.Classification, classification.Value)
            );
        }

        var documents = await _collection.Find(filter).ToListAsync();
        return documents.Select(ItemDocumentMapper.ToEntity);
    }

    public async Task<Item> CreateAsync(Item item)
    {
        var document = ItemDocumentMapper.ToDocument(item);
        var result = await UpsertAsync(null, document);
        return ItemDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(Item item)
    {
        var document = ItemDocumentMapper.ToDocument(item);
        await UpsertAsync(item.Id, document);
    }

    public async Task DeleteAsync(string id, string deletedBy, DateTime deletedAtUtc)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            item.IsDeleted = true;
            item.DeletedBy = deletedBy;
            item.DeletedAtUtc = deletedAtUtc;
            await UpdateAsync(item);
        }
    }
}
