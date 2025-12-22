using MongoDB.Driver;

namespace HenryTires.Inventory.Application.Ports;

public interface IMongoUnitOfWork
{
    Task<IClientSessionHandle> StartSessionAsync();
}
