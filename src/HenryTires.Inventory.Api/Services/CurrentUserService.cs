using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Enums;
using System.Security.Claims;

namespace HenryTires.Inventory.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
        _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value;

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    public Role? UserRole
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ??
                           _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;

            if (string.IsNullOrEmpty(roleClaim))
                return null;

            return Enum.TryParse<Role>(roleClaim, out var role) ? role : null;
        }
    }

    public string? BranchId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("branchId")?.Value;

    public string? BranchCode =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("branchCode")?.Value;
}
