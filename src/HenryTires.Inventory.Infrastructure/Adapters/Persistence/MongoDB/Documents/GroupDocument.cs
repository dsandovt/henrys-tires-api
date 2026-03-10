using HenryTires.Inventory.Domain.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class GroupDocument : AuditTrail
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required List<string> RoleReferences { get; set; }

    public required bool IsActive { get; set; }
}
