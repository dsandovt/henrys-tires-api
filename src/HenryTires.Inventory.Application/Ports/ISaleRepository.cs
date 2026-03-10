using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(string id);
    Task<IEnumerable<Sale>> GetByBranchAndDateRangeAsync(
        string branchReference,
        DateTime from,
        DateTime to
    );
    Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Sale>> SearchAsync(
        string? branchReference,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    );
    Task<int> CountAsync(string? branchReference, DateTime? from, DateTime? to);

    Task<Sale> CreateAsync(Sale sale);
    Task UpdateAsync(Sale sale);
}
