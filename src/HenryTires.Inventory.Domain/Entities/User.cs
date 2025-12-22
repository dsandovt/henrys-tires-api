using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Domain.Entities;

public class User : AuditTrail
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required Role Role { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? BranchId { get; set; }

    public required bool IsActive { get; set; }
}
