using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

/// <summary>
/// Sale repository interface. Basic CRUD operations are inherited from CrudRepository.
/// This interface defines sale-specific queries and commands.
/// </summary>
public interface ISaleRepository
{
    // Custom query methods (GetByIdAsync inherited from CrudRepository)
    Task<Sale?> GetByIdAsync(string id); // From CrudRepository base class
    Task<IEnumerable<Sale>> GetByBranchAndDateRangeAsync(string branchId, DateTime from, DateTime to);
    Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Sale>> SearchAsync(string? branchId, DateTime? from, DateTime? to, int page, int pageSize);
    Task<int> CountAsync(string? branchId, DateTime? from, DateTime? to);

    // Custom command methods
    Task<Sale> CreateAsync(Sale sale);
    Task UpdateAsync(Sale sale);
}
