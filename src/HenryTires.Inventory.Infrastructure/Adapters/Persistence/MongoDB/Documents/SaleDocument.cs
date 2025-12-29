using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class SaleDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string SaleNumber { get; set; }
    public required string BranchId { get; set; }
    public required DateTime SaleDateUtc { get; set; }
    public required List<SaleLineDocument> Lines { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }

    [BsonRepresentation(BsonType.String)]
    public TransactionStatus Status { get; set; }

    public DateTime? PostedAtUtc { get; set; }
    public string? PostedBy { get; set; }

    // AuditTrail
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class SaleLineDocument
{
    public string? LineId { get; set; }
    public required string ItemId { get; set; }
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

    public string? InventoryTransactionId { get; set; }
}
