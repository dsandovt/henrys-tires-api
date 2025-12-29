using HenryTires.Inventory.Application.Ports.Outbound;
using MongoDB.Bson;

namespace HenryTires.Inventory.Infrastructure.Adapters.Identity;

public class MongoIdentityGenerator : IIdentityGenerator
{
    public string GenerateId()
    {
        return ObjectId.GenerateNewId().ToString();
    }
}
