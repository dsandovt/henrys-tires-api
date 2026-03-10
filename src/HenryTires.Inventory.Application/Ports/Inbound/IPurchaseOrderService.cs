using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto request);
    Task<PurchaseOrderDto> ReceivePurchaseOrderAsync(string purchaseOrderId);
    Task<PurchaseOrderDto> CancelPurchaseOrderAsync(string purchaseOrderId);
    Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(string id);
    Task<PaginatedResponse<PurchaseOrderDto>> SearchPurchaseOrdersAsync(
        string? branchReference,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    );
}
