using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports.Inbound;

/// <summary>
/// Inbound port for sale operations.
/// </summary>
public interface ISaleService
{
    Task<Sale> CreateSaleAsync(
        string? branchId,
        DateTime saleDateUtc,
        List<SaleLine> lines,
        string? customerName,
        string? customerPhone,
        string? notes);

    Task<Sale> PostSaleAsync(string saleId);
    Task<Sale?> GetSaleByIdAsync(string saleId);
    Task<IEnumerable<Sale>> GetSalesByBranchAndDateRangeAsync(string branchId, DateTime from, DateTime to);
    Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Sale>> SearchSalesAsync(string? branchId, DateTime? from, DateTime? to, int page, int pageSize);
    Task<int> CountSalesAsync(string? branchId, DateTime? from, DateTime? to);
}
