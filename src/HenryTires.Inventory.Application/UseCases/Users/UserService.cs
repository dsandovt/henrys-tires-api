using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.UseCases.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IIdentityGenerator _identityGenerator;

    public UserService(
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IGroupRepository groupRepository,
        IPasswordHasher passwordHasher,
        IClock clock,
        ICurrentUser currentUser,
        IIdentityGenerator identityGenerator
    )
    {
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _groupRepository = groupRepository;
        _passwordHasher = passwordHasher;
        _clock = clock;
        _currentUser = currentUser;
        _identityGenerator = identityGenerator;
    }

    public async Task<PaginatedResponse<UserDto>> GetUsersAsync(int page, int pageSize, string? search)
    {
        var users = await _userRepository.SearchAsync(search, page, pageSize);
        var count = await _userRepository.CountAsync(search);

        return new PaginatedResponse<UserDto>
        {
            Items = users.Select(MapToDto),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with ID '{id}' not found");

        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Validate all group references exist
        var groups = await _groupRepository.GetByIdsAsync(request.GroupReferences);
        if (groups.Count() != request.GroupReferences.Count)
            throw new ValidationException("One or more group references not found");

        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            throw new ValidationException($"Username '{request.Username}' already exists");

        // Validate all branch references exist
        if (request.BranchReferences.Count > 0)
        {
            var branches = await _branchRepository.GetByIdsAsync(request.BranchReferences);
            if (branches.Count() != request.BranchReferences.Count)
                throw new ValidationException("One or more branch references not found");
        }

        var user = new User
        {
            Id = _identityGenerator.GenerateId(),
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            SecondLastName = request.SecondLastName,
            Email = request.Email,
            GroupReferences = request.GroupReferences,
            BranchReferences = request.BranchReferences,
            IsActive = request.IsActive,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
        };

        await _userRepository.CreateAsync(user);

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with ID '{id}' not found");

        if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                throw new ValidationException($"Username '{request.Username}' already exists");
            user.Username = request.Username;
        }

        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = _passwordHasher.Hash(request.Password);

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.MiddleName != null) user.MiddleName = request.MiddleName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.SecondLastName != null) user.SecondLastName = request.SecondLastName;
        if (request.Email != null) user.Email = request.Email;

        if (request.GroupReferences != null)
        {
            var groups = await _groupRepository.GetByIdsAsync(request.GroupReferences);
            if (groups.Count() != request.GroupReferences.Count)
                throw new ValidationException("One or more group references not found");
            user.GroupReferences = request.GroupReferences;
        }

        if (request.BranchReferences != null)
        {
            if (request.BranchReferences.Count > 0)
            {
                var branches = await _branchRepository.GetByIdsAsync(request.BranchReferences);
                if (branches.Count() != request.BranchReferences.Count)
                    throw new ValidationException("One or more branch references not found");
            }
            user.BranchReferences = request.BranchReferences;
        }

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.ModifiedAtUtc = _clock.UtcNow;
        user.ModifiedBy = _currentUser.Username;

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with ID '{id}' not found");

        if (user.Username == _currentUser.Username)
            throw new BusinessException("Cannot delete your own user account");

        await _userRepository.DeleteAsync(id);
    }

    public async Task<UserDto> ToggleUserStatusAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with ID '{id}' not found");

        if (user.Username == _currentUser.Username)
            throw new BusinessException("Cannot deactivate your own user account");

        user.IsActive = !user.IsActive;
        user.ModifiedAtUtc = _clock.UtcNow;
        user.ModifiedBy = _currentUser.Username;

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    public async Task ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID '{userId}' not found");

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.ModifiedAtUtc = _clock.UtcNow;
        user.ModifiedBy = _currentUser.Username;

        await _userRepository.UpdateAsync(user);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            SecondLastName = user.SecondLastName,
            Email = user.Email,
            GroupReferences = user.GroupReferences,
            BranchReferences = user.BranchReferences,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            CreatedBy = user.CreatedBy,
        };
    }
}
