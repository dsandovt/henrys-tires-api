using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class InventoryTransactionDocument : AuditTrail
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string BranchReference { get; set; }

    public required string BranchCode { get; set; }
    public required EntityKeyDocument Initiator { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required InventoryTransactionStatus Status { get; set; }

    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public required List<InventoryTransactionLineDocument> Lines { get; set; }
    public required List<StatusHistoryEntryDocument<InventoryTransactionStatus>> StatusHistory { get; set; }
}

public class EntityKeyDocument
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Reference { get; set; }
    public required string ReferenceNumber { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required InitiatorType EntityDefinitionCode { get; set; }
}

public class InventoryTransactionLineDocument
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string LineId { get; set; }

    public required string ItemCode { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required ItemCondition Condition { get; set; }

    public required int Quantity { get; set; }
}

public class StatusHistoryEntryDocument<T>
{
    public required DateTime Date { get; set; }

    [BsonRepresentation(BsonType.Int32)]
    public required T Status { get; set; }

    public required UserLiteDocument User { get; set; }
    public string? Comment { get; set; }
}

public class UserLiteDocument
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? SecondLastName { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
}
