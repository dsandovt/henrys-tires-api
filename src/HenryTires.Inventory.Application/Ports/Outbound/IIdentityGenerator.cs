namespace HenryTires.Inventory.Application.Ports.Outbound;

/// <summary>
/// Port for generating unique identifiers.
/// Abstracts away the underlying ID generation strategy (MongoDB ObjectId, GUID, etc.)
/// </summary>
public interface IIdentityGenerator
{
    /// <summary>
    /// Generates a new unique identifier as a string.
    /// </summary>
    string GenerateId();
}
