using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HenryTires.Inventory.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IBranchRepository _branchRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IRoleRepository _roleRepository;

    public JwtTokenService(
        IConfiguration configuration,
        IBranchRepository branchRepository,
        IGroupRepository groupRepository,
        IRoleRepository roleRepository
    )
    {
        _configuration = configuration;
        _branchRepository = branchRepository;
        _groupRepository = groupRepository;
        _roleRepository = roleRepository;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not configured");
        var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience not configured");
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "480");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            // Add simple claim names for frontend JWT decoding
            new Claim("nameid", user.Username),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
        };

        if (!string.IsNullOrEmpty(user.MiddleName))
            claims.Add(new Claim("middleName", user.MiddleName));
        if (!string.IsNullOrEmpty(user.SecondLastName))
            claims.Add(new Claim("secondLastName", user.SecondLastName));
        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

        // Add one claim per group reference
        foreach (var groupRef in user.GroupReferences)
        {
            claims.Add(new Claim("groupReference", groupRef));
        }

        // Load ALL groups and union role codes
        var groups = await _groupRepository.GetByIdsAsync(user.GroupReferences);
        var addedRoleCodes = new HashSet<string>();
        foreach (var group in groups)
        {
            if (!group.IsActive) continue;
            foreach (var roleId in group.RoleReferences)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null && role.IsActive && addedRoleCodes.Add(role.Code))
                {
                    claims.Add(new Claim("roleCodes", role.Code));
                }
            }
        }

        // Add branch information — one pair of claims per branch
        if (user.BranchReferences.Count > 0)
        {
            var branches = await _branchRepository.GetByIdsAsync(user.BranchReferences);
            foreach (var branch in branches)
            {
                claims.Add(new Claim("branchReference", branch.Id));
                claims.Add(new Claim("branchCode", branch.Code));
                claims.Add(new Claim("branchName", branch.Name));
            }
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
