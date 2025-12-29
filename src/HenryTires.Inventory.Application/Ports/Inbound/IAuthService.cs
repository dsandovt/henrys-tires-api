using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

/// <summary>
/// Inbound port for authentication and authorization operations.
/// </summary>
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task SeedDevDataAsync();
}
