using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public class ConsumableItemPriceRepository : CrudRepository<ConsumableItemPrice>, IConsumableItemPriceRepository
{
    public ConsumableItemPriceRepository(IMongoClient client)
        : base(client, "Inventory", "ConsumableItemPrice")
    {
        var indexKeys = Builders<ConsumableItemPrice>.IndexKeys.Ascending(p => p.ItemCode);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<ConsumableItemPrice>(indexKeys, indexOptions);
        _collection.Indexes.CreateOneAsync(indexModel);
    }

    public async Task<ConsumableItemPrice?> GetByItemCodeAsync(string itemCode)
    {
        return await _collection.Find(p => p.ItemCode == itemCode).FirstOrDefaultAsync();
    }

    public async Task<ConsumableItemPrice> CreateAsync(ConsumableItemPrice price)
    {
        return await UpsertAsync(null, price);
    }

    public async Task UpdateAsync(ConsumableItemPrice price)
    {
        await UpsertAsync(price.Id, price);
    }
}
