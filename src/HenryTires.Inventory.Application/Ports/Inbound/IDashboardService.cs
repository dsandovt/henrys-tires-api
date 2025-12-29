using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface IDashboardService
{
    Task<DashboardDataDto> GetDashboardDataAsync(
        DateTime startDateUtc,
        DateTime endDateUtc,
        string? branchCode = null
    );
}
