using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Username { get; set; }
    public required string PasswordHash { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required Role Role { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? BranchId { get; set; }

    public required bool IsActive { get; set; }

    // AuditTrail
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}
