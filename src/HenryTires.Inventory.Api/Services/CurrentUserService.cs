using HenryTires.Inventory.Application.Ports;
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

    public IReadOnlyList<string>? GroupReferences
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("groupReference");
            if (claims == null || !claims.Any())
                return null;
            return claims.Select(c => c.Value).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<string>? RoleCodes
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("roleCodes");
            if (claims == null || !claims.Any())
                return null;
            return claims.Select(c => c.Value).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<string>? BranchReferences
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("branchReference");
            if (claims == null || !claims.Any())
                return null;
            return claims.Select(c => c.Value).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<string>? BranchCodes
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("branchCode");
            if (claims == null || !claims.Any())
                return null;
            return claims.Select(c => c.Value).ToList().AsReadOnly();
        }
    }
}
