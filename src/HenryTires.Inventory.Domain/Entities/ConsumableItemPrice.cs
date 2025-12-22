using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Domain.Entities;

public class ConsumableItemPrice
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
    public required string ItemCode { get; set; }
    public required string Currency { get; set; }
    public required decimal LatestPrice { get; set; }
    public required DateTime LatestPriceDateUtc { get; set; }
    public required string UpdatedBy { get; set; }
    public required List<PriceHistoryEntry> History { get; set; }

    public void UpdatePrice(decimal newPrice, string updatedBy, DateTime dateUtc)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(newPrice));

        History.Add(
            new PriceHistoryEntry
            {
                Price = LatestPrice,
                DateUtc = LatestPriceDateUtc,
                UpdatedBy = UpdatedBy,
            }
        );

        LatestPrice = newPrice;
        LatestPriceDateUtc = dateUtc;
        UpdatedBy = updatedBy;
    }
}

public class PriceHistoryEntry
{
    public required decimal Price { get; set; }
    public required DateTime DateUtc { get; set; }
    public required string UpdatedBy { get; set; }
}
