using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Domain.Entities;

public class InventoryBalance
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
    public required string BranchId { get; set; }
    public required string ItemId { get; set; }
    public required string ItemCode { get; set; }
    public required ItemCondition Condition { get; set; }
    public required int QuantityOnHand { get; set; }
    public required DateTime LastUpdatedUtc { get; set; }
}
