using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface ISaleService
{
    Task<Sale> CreateSaleAsync(CreateSaleRequest request);
    Task<Sale> PostSaleAsync(string saleId);
    Task<Sale?> GetSaleByIdAsync(string saleId);
    Task<IEnumerable<Sale>> GetSalesByBranchAndDateRangeAsync(
        string branchId,
        DateTime from,
        DateTime to
    );
    Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Sale>> SearchSalesAsync(
        string? branchId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    );
    Task<int> CountSalesAsync(string? branchId, DateTime? from, DateTime? to);
}
