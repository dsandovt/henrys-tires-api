namespace HenryTires.Inventory.Application.Ports.Outbound;

/// <summary>
/// Port for generating unique sequential numbers (counters)
/// </summary>
public interface ISequenceGenerator
{
    /// <summary>
    /// Gets the next sequence number for a given sequence name
    /// </summary>
    /// <param name="sequenceName">The name of the sequence (e.g., "sale-{branchCode}")</param>
    /// <param name="transactionScope">Optional transaction scope for atomicity</param>
    /// <returns>The next sequence number</returns>
    Task<long> GetNextSequenceAsync(string sequenceName, ITransactionScope? transactionScope = null);
}
