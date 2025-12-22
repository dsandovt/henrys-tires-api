using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Domain.Entities;

public class Sale : AuditTrail
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
    public required string SaleNumber { get; set; }
    public required string BranchId { get; set; }
    public required DateTime SaleDateUtc { get; set; }
    public required List<SaleLine> Lines { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime? PostedAtUtc { get; set; }
    public string? PostedBy { get; set; }
}
