using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task SeedDevDataAsync();
}
