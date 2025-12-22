using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Infrastructure.Data;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Services;

public class MongoUnitOfWork : IMongoUnitOfWork
{
    private readonly MongoDbContext _context;

    public MongoUnitOfWork(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IClientSessionHandle> StartSessionAsync()
    {
        return await _context.Database.Client.StartSessionAsync();
    }
}
