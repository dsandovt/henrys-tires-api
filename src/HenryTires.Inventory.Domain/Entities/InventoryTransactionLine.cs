using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Domain.Entities;

public class InventoryTransactionLine
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string LineId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string ItemId { get; set; }
    public required string ItemCode { get; set; }

    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public required ItemCondition Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required string Currency { get; set; }

    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public required PriceSource PriceSource { get; set; }
    public required string PriceSetByRole { get; set; }
    public required string PriceSetByUser { get; set; }
    public required decimal LineTotal { get; set; }
    public decimal? CostOfGoodsSold { get; set; }
    public string? PriceNotes { get; set; }
    public required DateTime ExecutedAtUtc { get; set; }

    public static decimal CalculateLineTotal(int quantity, decimal unitPrice)
    {
        return quantity * unitPrice;
    }
}
