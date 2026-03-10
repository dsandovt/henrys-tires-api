using System.Security.Claims;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.ValueObjects;
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

    public string FirstName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("firstName")?.Value ?? "";

    public string LastName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("lastName")?.Value ?? "";

    public string? MiddleName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("middleName")?.Value;

    public string? SecondLastName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("secondLastName")?.Value;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public IReadOnlyList<string> GroupReferences
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("groupReference");
            return claims?.Select(c => c.Value).ToList().AsReadOnly()
                ?? new List<string>().AsReadOnly();
        }
    }

    public IReadOnlyList<string> RoleCodes
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("roleCodes");
            return claims?.Select(c => c.Value).ToList().AsReadOnly()
                ?? new List<string>().AsReadOnly();
        }
    }

    public IReadOnlyList<string> BranchReferences
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("branchReference");
            return claims?.Select(c => c.Value).ToList().AsReadOnly()
                ?? new List<string>().AsReadOnly();
        }
    }

    public IReadOnlyList<string> BranchCodes
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll("branchCode");
            return claims?.Select(c => c.Value).ToList().AsReadOnly()
                ?? new List<string>().AsReadOnly();
        }
    }

    public bool HasRole(string roleCode)
    {
        return RoleCodes.Contains(roleCode);
    }

    public bool CanAccessBranch(string branchReference)
    {
        return BranchReferences.Contains(branchReference);
    }

    public bool CanAccessBranchCode(string branchCode)
    {
        return BranchCodes.Contains(branchCode);
    }

    public UserLite ToUserLite() => new()
    {
        FirstName = FirstName,
        MiddleName = MiddleName,
        LastName = LastName,
        SecondLastName = SecondLastName,
        Username = Username,
        Email = Email,
    };
}
