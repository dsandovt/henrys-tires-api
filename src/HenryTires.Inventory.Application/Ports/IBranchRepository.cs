using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(string id);
    Task<IEnumerable<Branch>> GetAllAsync();

    Task<Branch?> GetByCodeAsync(string code);
    Task<IEnumerable<Branch>> GetByIdsAsync(List<string> ids);
}
