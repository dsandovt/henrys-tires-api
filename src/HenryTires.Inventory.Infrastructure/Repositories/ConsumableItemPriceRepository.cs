using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class ConsumableItemPriceRepository : CrudRepository<ConsumableItemPriceDocument>, IConsumableItemPriceRepository
{
    public ConsumableItemPriceRepository(IMongoClient client)
        : base(client, "Inventory", "ConsumableItemPrice")
    {
        var indexKeys = Builders<ConsumableItemPriceDocument>.IndexKeys.Ascending(p => p.ItemCode);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<ConsumableItemPriceDocument>(indexKeys, indexOptions);
        _collection.Indexes.CreateOneAsync(indexModel);
    }

    public async Task<ConsumableItemPrice?> GetByItemCodeAsync(string itemCode)
    {
        var document = await _collection.Find(p => p.ItemCode == itemCode).FirstOrDefaultAsync();
        return document == null ? null : ConsumableItemPriceDocumentMapper.ToEntity(document);
    }

    public new async Task<ConsumableItemPrice?> GetByIdAsync(string id)
    {
        var document = await base.GetByIdAsync(id);
        return document == null ? null : ConsumableItemPriceDocumentMapper.ToEntity(document);
    }

    public async Task<ConsumableItemPrice> CreateAsync(ConsumableItemPrice price)
    {
        var document = ConsumableItemPriceDocumentMapper.ToDocument(price);
        var result = await UpsertAsync(null, document);
        return ConsumableItemPriceDocumentMapper.ToEntity(result);
    }

    public async Task UpdateAsync(ConsumableItemPrice price)
    {
        var document = ConsumableItemPriceDocumentMapper.ToDocument(price);
        await UpsertAsync(price.Id, document);
    }

    public new async Task<IEnumerable<ConsumableItemPrice>> GetAllAsync()
    {
        var documents = await base.GetAllAsync();
        return documents.Select(ConsumableItemPriceDocumentMapper.ToEntity);
    }
}
