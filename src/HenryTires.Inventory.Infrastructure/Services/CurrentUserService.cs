using System.Security.Claims;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HenryTires.Inventory.Infrastructure.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserService> logger
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(
                ClaimTypes.NameIdentifier
            );
            return claim?.Value
                ?? throw new InvalidOperationException("User ID not found in claims");
        }
    }

    public string Username
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name);
            return claim?.Value
                ?? throw new InvalidOperationException("Username not found in claims");
        }
    }

    public Role Role
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            if (claim == null || !Enum.TryParse<Role>(claim.Value, out var role))
            {
                throw new InvalidOperationException("Role not found in claims");
            }
            return role;
        }
    }

    public string? BranchId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("BranchId");
            return claim?.Value;
        }
    }

    public string? BranchCode
    {
        get
        {
            // BranchCode is stored in JWT claims by JwtTokenService
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("branchCode");
            var branchCode = claim?.Value;

            if (branchCode == null && BranchId != null)
            {
                _logger.LogWarning(
                    "BranchCode claim not found for user {UserId} with BranchId {BranchId}. "
                        + "This may indicate an old JWT token. User should re-login.",
                    UserId,
                    BranchId
                );
            }

            return branchCode;
        }
    }
}
