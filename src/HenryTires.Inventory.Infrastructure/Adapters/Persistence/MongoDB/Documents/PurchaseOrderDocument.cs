using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class PurchaseOrderDocument : AuditTrail
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Number { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string BranchReference { get; set; }

    public required string BranchCode { get; set; }

    public required DateTime OrderDateUtc { get; set; }
    public required List<PurchaseOrderLineDocument> Lines { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required PurchaseOrderStatus Status { get; set; }

    public required List<StatusHistoryEntryDocument<PurchaseOrderStatus>> StatusHistory { get; set; }
}

public class PurchaseOrderLineDocument
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string LineId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string ItemReference { get; set; }

    public required string ItemCode { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required ItemCondition Condition { get; set; }

    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required Currency Currency { get; set; }
}
