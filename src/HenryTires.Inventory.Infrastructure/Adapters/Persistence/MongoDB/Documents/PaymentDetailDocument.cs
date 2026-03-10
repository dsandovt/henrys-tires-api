using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

public class PaymentDetailDocument
{
    [BsonRepresentation(BsonType.String)]
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? CheckNumber { get; set; }
}
