using HenryTires.Inventory.Application.Ports.Outbound;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Adapters.Transactions;

/// <summary>
/// MongoDB implementation of IUnitOfWork.
/// Manages MongoDB client sessions for transaction coordination.
/// </summary>
public class MongoUnitOfWork : IUnitOfWork
{
    private readonly IMongoClient _mongoClient;

    public MongoUnitOfWork(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
    }

    public async Task<ITransactionScope> BeginTransactionAsync()
    {
        var session = await _mongoClient.StartSessionAsync();
        session.StartTransaction();
        return new MongoTransactionScope(session);
    }
}
