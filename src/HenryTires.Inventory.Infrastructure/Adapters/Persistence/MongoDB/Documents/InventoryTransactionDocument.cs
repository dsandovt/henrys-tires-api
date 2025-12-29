using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class InventoryTransactionDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string TransactionNumber { get; set; }
    public required string BranchCode { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required TransactionType Type { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required TransactionStatus Status { get; set; }

    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }

    [BsonRepresentation(BsonType.String)]
    public PaymentMethod? PaymentMethod { get; set; }

    public DateTime? CommittedAtUtc { get; set; }
    public string? CommittedBy { get; set; }
    public required List<InventoryTransactionLineDocument> Lines { get; set; }

    // AuditTrail
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class InventoryTransactionLineDocument
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string LineId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string ItemId { get; set; }

    public required string ItemCode { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required ItemCondition Condition { get; set; }

    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required Currency Currency { get; set; }

    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;

    [BsonRepresentation(BsonType.String)]
    public required PriceSource PriceSource { get; set; }

    public required string PriceSetByRole { get; set; }
    public required string PriceSetByUser { get; set; }
    public required decimal LineTotal { get; set; }
    public decimal? CostOfGoodsSold { get; set; }
    public string? PriceNotes { get; set; }
    public required DateTime ExecutedAtUtc { get; set; }
}
