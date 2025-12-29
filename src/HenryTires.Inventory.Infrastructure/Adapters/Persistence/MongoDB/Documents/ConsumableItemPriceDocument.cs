using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class ConsumableItemPriceDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string ItemCode { get; set; }

    [BsonRepresentation(BsonType.String)]
    public required Currency Currency { get; set; }

    public required decimal LatestPrice { get; set; }
    public required DateTime LatestPriceDateUtc { get; set; }
    public required string UpdatedBy { get; set; }
    public required List<PriceHistoryEntryDocument> History { get; set; }
}

public class PriceHistoryEntryDocument
{
    public required decimal Price { get; set; }
    public required DateTime DateUtc { get; set; }
    public required string UpdatedBy { get; set; }
}
