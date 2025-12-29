using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;
using HenryTires.Inventory.Infrastructure.Adapters.Transactions;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Repositories;

public class MongoSequenceGenerator : ISequenceGenerator
{
    private readonly IMongoCollection<SequenceDocument> _collection;

    public MongoSequenceGenerator(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("Inventory");
        _collection = database.GetCollection<SequenceDocument>("Sequence");
    }

    public async Task<long> GetNextSequenceAsync(string sequenceName, ITransactionScope? transactionScope = null)
    {
        var filter = Builders<SequenceDocument>.Filter.Eq(s => s.Id, sequenceName);
        var update = Builders<SequenceDocument>.Update.Inc(s => s.CurrentValue, 1);
        var options = new FindOneAndUpdateOptions<SequenceDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        SequenceDocument result;

        if (transactionScope is MongoTransactionScope mongoScope && mongoScope.Session != null)
        {
            // Use the existing transaction session
            result = await _collection.FindOneAndUpdateAsync(
                mongoScope.Session,
                filter,
                update,
                options
            );
        }
        else
        {
            // No transaction - standalone atomic operation
            result = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                options
            );
        }

        return result.CurrentValue;
    }
}
