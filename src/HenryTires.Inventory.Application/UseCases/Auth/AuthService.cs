using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.UseCases.Auth;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IClock _clock;
    private readonly IIdentityGenerator _identityGenerator;

    public AuthService(
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IClock clock,
        IIdentityGenerator identityGenerator
    )
    {
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _clock = clock;
        _identityGenerator = identityGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        var token = await _jwtTokenService.GenerateTokenAsync(user);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.ToString(),
            BranchId = user.BranchId,
        };
    }

    public async Task SeedDevDataAsync()
    {
        var existingAdmin = await _userRepository.GetByUsernameAsync("admin");
        if (existingAdmin != null)
        {
            return;
        }

        var branches = await _branchRepository.GetAllAsync();
        var branchList = branches.ToList();

        var admin = new User
        {
            Id = _identityGenerator.GenerateId(),
            Username = "admin",
            PasswordHash = _passwordHasher.Hash("admin123"),
            Role = Role.Admin,
            BranchId = null,
            IsActive = true,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = "system",
        };
        await _userRepository.CreateAsync(admin);

        var branchUsernames = new[]
        {
            "mercury",
            "williamsburg",
            "warwick",
            "jefferson",
            "pembroke",
        };
        foreach (var username in branchUsernames)
        {
            var branch = branchList.FirstOrDefault(b => b.Code.ToLower() == username.ToUpper());
            if (branch != null)
            {
                var user = new User
                {
                    Id = _identityGenerator.GenerateId(),
                    Username = username,
                    PasswordHash = _passwordHasher.Hash(username + "123"),
                    Role = Role.Seller,
                    BranchId = branch.Id,
                    IsActive = true,
                    CreatedAtUtc = _clock.UtcNow,
                    CreatedBy = "system",
                };
                await _userRepository.CreateAsync(user);
            }
        }
    }
}
