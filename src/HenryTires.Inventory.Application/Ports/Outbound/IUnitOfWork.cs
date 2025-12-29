namespace HenryTires.Inventory.Application.Ports.Outbound;

public interface IUnitOfWork
{
    Task<ITransactionScope> BeginTransactionAsync();
}
