using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class SaleDocument : AuditTrail
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Number { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string BranchReference { get; set; }

    public required string BranchCode { get; set; }

    public required DateTime SaleDateUtc { get; set; }
    public required List<SaleLineDocument> Lines { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required PaymentMethod PaymentMethod { get; set; }

    public List<PaymentDetailDocument>? PaymentDetails { get; set; }

    [BsonRepresentation(BsonType.String)]
    public SaleStatus Status { get; set; }

    public required List<StatusHistoryEntryDocument<SaleStatus>> StatusHistory { get; set; }
}

public class SaleLineDocument
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string? LineId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string ItemReference { get; set; }

    public required string ItemCode { get; set; }
    public required string Description { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required Classification Classification { get; set; }

    [BsonRepresentation(BsonType.String)]
    public ItemCondition? Condition { get; set; }

    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required Currency Currency { get; set; }

    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;
}
