using HenryTires.Inventory.Domain.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class UserDocument : AuditTrail
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? SecondLastName { get; set; }
    public string? Email { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required List<string> GroupReferences { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required List<string> BranchReferences { get; set; }

    public required bool IsActive { get; set; }
}
