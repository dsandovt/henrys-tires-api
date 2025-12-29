using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class InventoryBalanceDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string BranchId { get; set; }
    public required string ItemId { get; set; }
    public required string ItemCode { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required ItemCondition Condition { get; set; }

    public required int QuantityOnHand { get; set; }
    public required DateTime LastUpdatedUtc { get; set; }
}
