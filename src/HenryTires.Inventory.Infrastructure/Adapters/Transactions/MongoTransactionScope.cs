using HenryTires.Inventory.Application.Ports.Outbound;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Adapters.Transactions;

/// <summary>
/// MongoDB implementation of ITransactionScope.
/// Wraps MongoDB's IClientSessionHandle to provide a framework-agnostic transaction interface.
/// </summary>
public class MongoTransactionScope : ITransactionScope
{
    private readonly IClientSessionHandle _session;
    private bool _disposed;

    public MongoTransactionScope(IClientSessionHandle session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    /// <summary>
    /// Gets the underlying MongoDB session handle.
    /// Internal use only - for repositories that need to participate in the transaction.
    /// </summary>
    internal IClientSessionHandle Session => _session;

    public async Task CommitAsync()
    {
        await _session.CommitTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        await _session.AbortTransactionAsync();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _session?.Dispose();
        _disposed = true;
    }
}
