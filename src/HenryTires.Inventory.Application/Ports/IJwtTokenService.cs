using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(User user);
}
