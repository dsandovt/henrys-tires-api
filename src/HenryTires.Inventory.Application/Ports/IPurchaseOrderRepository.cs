using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(string id);
    Task<IEnumerable<PurchaseOrder>> GetByBranchAsync(string branchReference, int page, int pageSize);
    Task<long> CountByBranchAsync(string? branchReference);
    Task<IEnumerable<PurchaseOrder>> SearchAsync(
        string? branchReference,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    );
    Task<int> CountAsync(string? branchReference, DateTime? from, DateTime? to);
    Task<PurchaseOrder> CreateAsync(PurchaseOrder purchaseOrder);
    Task UpdateAsync(PurchaseOrder purchaseOrder);
}
