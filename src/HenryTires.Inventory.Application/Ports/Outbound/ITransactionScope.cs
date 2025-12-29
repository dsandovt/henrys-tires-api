namespace HenryTires.Inventory.Application.Ports.Outbound;

public interface ITransactionScope : IDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
