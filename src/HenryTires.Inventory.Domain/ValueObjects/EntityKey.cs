using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.ValueObjects;

public class EntityKey
{
    public required string Reference { get; set; }
    public required string ReferenceNumber { get; set; }
    public required InitiatorType EntityDefinitionCode { get; set; }
}
