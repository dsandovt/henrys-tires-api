using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.UseCases.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IClock _clock;
    private readonly IIdentityGenerator _identityGenerator;

    public AuthService(
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IGroupRepository groupRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IClock clock,
        IIdentityGenerator identityGenerator
    )
    {
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _groupRepository = groupRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _clock = clock;
        _identityGenerator = identityGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !user.IsActive)
            throw new UnauthorizedException("Invalid credentials");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        // Load ALL groups to union role codes
        var groups = await _groupRepository.GetByIdsAsync(user.GroupReferences);
        var roleCodes = new List<string>();

        foreach (var group in groups)
        {
            if (!group.IsActive) continue;
            foreach (var roleId in group.RoleReferences)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null && role.IsActive && !roleCodes.Contains(role.Code))
                    roleCodes.Add(role.Code);
            }
        }

        var token = await _jwtTokenService.GenerateTokenAsync(user);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            GroupReferences = user.GroupReferences,
            RoleCodes = roleCodes,
            BranchReferences = user.BranchReferences,
        };
    }

    public async Task SeedDevDataAsync()
    {
        var existingAdmin = await _userRepository.GetByUsernameAsync("admin");
        if (existingAdmin != null)
            return;

        var branches = await _branchRepository.GetAllAsync();
        var branchList = branches.ToList();

        var admin = new User
        {
            Id = _identityGenerator.GenerateId(),
            Username = "admin",
            PasswordHash = _passwordHasher.Hash("admin123"),
            FirstName = "Admin",
            LastName = "User",
            GroupReferences = ["group-admin"],
            BranchReferences = [],
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
                    FirstName = char.ToUpper(username[0]) + username[1..],
                    LastName = "User",
                    GroupReferences = ["group-seller"],
                    BranchReferences = [branch.Id],
                    IsActive = true,
                    CreatedAtUtc = _clock.UtcNow,
                    CreatedBy = "system",
                };
                await _userRepository.CreateAsync(user);
            }
        }
    }
}
