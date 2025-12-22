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

    public JwtTokenService(IConfiguration configuration, IBranchRepository branchRepository)
    {
        _configuration = configuration;
        _branchRepository = branchRepository;
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
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            // Add simple claim names for frontend JWT decoding
            new Claim("nameid", user.Username),
            new Claim("role", user.Role.ToString())
        };

        // Add branch information if user has a branch
        if (!string.IsNullOrEmpty(user.BranchId))
        {
            claims.Add(new Claim("branchId", user.BranchId));

            // Fetch branch to get the name and code
            var branch = await _branchRepository.GetByIdAsync(user.BranchId);
            if (branch != null)
            {
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
