namespace HenryTires.Inventory.Application.Ports.Outbound;

/// <summary>
/// Represents a database transaction scope.
/// Provides an abstraction over the underlying transaction mechanism.
/// </summary>
public interface ITransactionScope : IDisposable
{
    /// <summary>
    /// Commits the transaction.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    Task RollbackAsync();
}
