using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class InventoryAdjustmentDocument : AuditTrail
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Number { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required AdjustmentType AdjustmentType { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required InventoryAdjustmentStatus Status { get; set; }

    public required List<StatusHistoryEntryDocument<InventoryAdjustmentStatus>> StatusHistory { get; set; }

    public required DateTime AdjustmentDateUtc { get; set; }
    public string? Notes { get; set; }

    // BranchTransfer
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? OriginBranchReference { get; set; }

    public string? OriginBranchCode { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? DestinationBranchReference { get; set; }

    public string? DestinationBranchCode { get; set; }

    // StockCorrection
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? BranchReference { get; set; }

    public string? BranchCode { get; set; }

    [BsonRepresentation(BsonType.String)]
    [BsonIgnoreIfNull]
    public CorrectionDirection? Direction { get; set; }

    public required List<InventoryAdjustmentLineDocument> Lines { get; set; }
}

public class InventoryAdjustmentLineDocument
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string LineId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string ItemReference { get; set; }

    public required string ItemCode { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required ItemCondition Condition { get; set; }

    public required int Quantity { get; set; }
    public string? Notes { get; set; }
}
