using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.UseCases.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IIdentityGenerator _identityGenerator;

    public UserService(
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IPasswordHasher passwordHasher,
        IClock clock,
        ICurrentUser currentUser,
        IIdentityGenerator identityGenerator
    )
    {
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _passwordHasher = passwordHasher;
        _clock = clock;
        _currentUser = currentUser;
        _identityGenerator = identityGenerator;
    }

    public async Task<UserListResponse> GetUsersAsync(int page, int pageSize, string? search)
    {
        var users = await _userRepository.SearchAsync(search, page, pageSize);
        var count = await _userRepository.CountAsync(search);

        return new UserListResponse
        {
            Items = users.Select(MapToDto),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{id}' not found");
        }

        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        if (!Enum.TryParse<Role>(request.Role, true, out var role))
        {
            throw new ValidationException($"Invalid role: {request.Role}");
        }

        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            throw new ValidationException($"Username '{request.Username}' already exists");
        }

        if ((role == Role.Seller || role == Role.StoreSeller) && string.IsNullOrEmpty(request.BranchId))
        {
            throw new ValidationException($"BranchId is required for {role} role");
        }

        if (!string.IsNullOrEmpty(request.BranchId))
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
            {
                throw new ValidationException($"Branch with ID '{request.BranchId}' not found");
            }
        }

        var user = new User
        {
            Id = _identityGenerator.GenerateId(),
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            BranchId = (role == Role.Seller || role == Role.StoreSeller) ? request.BranchId : null,
            IsActive = request.IsActive,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
        };

        await _userRepository.CreateAsync(user);

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{id}' not found");
        }

        if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                throw new ValidationException($"Username '{request.Username}' already exists");
            }
            user.Username = request.Username;
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            if (!Enum.TryParse<Role>(request.Role, true, out var role))
            {
                throw new ValidationException($"Invalid role: {request.Role}");
            }
            user.Role = role;
        }

        if (user.Role == Role.Seller || user.Role == Role.StoreSeller)
        {
            if (request.BranchId != null)
            {
                if (!string.IsNullOrEmpty(request.BranchId))
                {
                    var branch = await _branchRepository.GetByIdAsync(request.BranchId);
                    if (branch == null)
                    {
                        throw new ValidationException(
                            $"Branch with ID '{request.BranchId}' not found"
                        );
                    }
                }
                user.BranchId = request.BranchId;
            }
            else if (string.IsNullOrEmpty(user.BranchId))
            {
                throw new ValidationException($"BranchId is required for {user.Role} role");
            }
        }
        else
        {
            user.BranchId = null;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        user.ModifiedAtUtc = _clock.UtcNow;
        user.ModifiedBy = _currentUser.Username;

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{id}' not found");
        }

        if (user.Username == _currentUser.Username)
        {
            throw new BusinessException("Cannot delete your own user account");
        }

        await _userRepository.DeleteAsync(id);
    }

    public async Task<UserDto> ToggleUserStatusAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{id}' not found");
        }

        if (user.Username == _currentUser.Username)
        {
            throw new BusinessException("Cannot deactivate your own user account");
        }

        user.IsActive = !user.IsActive;
        user.ModifiedAtUtc = _clock.UtcNow;
        user.ModifiedBy = _currentUser.Username;

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role.ToString(),
            BranchId = user.BranchId,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            CreatedBy = user.CreatedBy,
        };
    }
}
