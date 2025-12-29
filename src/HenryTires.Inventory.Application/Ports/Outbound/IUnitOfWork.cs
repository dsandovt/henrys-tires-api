namespace HenryTires.Inventory.Application.Ports.Outbound;

/// <summary>
/// Unit of Work pattern for coordinating database transactions.
/// Abstracts away the underlying transaction implementation (MongoDB sessions, EF transactions, etc.)
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Begins a new transaction scope.
    /// </summary>
    Task<ITransactionScope> BeginTransactionAsync();
}
