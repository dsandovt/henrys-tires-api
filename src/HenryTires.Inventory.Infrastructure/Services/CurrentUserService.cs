using System.Security.Claims;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HenryTires.Inventory.Infrastructure.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBranchRepository _branchRepository;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IBranchRepository branchRepository
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _branchRepository = branchRepository;
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
            if (BranchId == null)
            {
                Console.WriteLine("BranchCode: BranchId is null");
                return null;
            }

            try
            {
                Console.WriteLine(
                    $"BranchCode: Looking up branch for BranchId: '{BranchId}' (Length: {BranchId.Length})"
                );

                // Look up branch by ID to get Code
                var branch = _branchRepository.GetByIdAsync(BranchId).GetAwaiter().GetResult();

                if (branch == null)
                {
                    // Log warning but don't throw - let calling code handle missing branch
                    Console.WriteLine($"Warning: Branch not found for BranchId: {BranchId}");

                    // Try to get all branches to debug
                    var allBranches = _branchRepository.GetAllAsync().GetAwaiter().GetResult();
                    Console.WriteLine(
                        $"Available branches: {string.Join(", ", allBranches.Select(b => $"{b.Id} ({b.Code})"))}"
                    );

                    return null;
                }

                Console.WriteLine(
                    $"BranchCode: Found branch {branch.Code} for BranchId {BranchId}"
                );
                return branch.Code;
            }
            catch (Exception ex)
            {
                // Log error but don't throw - let calling code handle missing branch
                Console.WriteLine($"Error looking up branch for BranchId {BranchId}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
