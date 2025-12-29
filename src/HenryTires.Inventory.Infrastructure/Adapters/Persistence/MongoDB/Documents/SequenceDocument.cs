using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class SequenceDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required string Id { get; set; } // Sequence name (e.g., "sale-WARWICK")

    public long CurrentValue { get; set; } // Current sequence value
}
