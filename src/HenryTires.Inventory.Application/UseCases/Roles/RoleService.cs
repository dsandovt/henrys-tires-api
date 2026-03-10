using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;

using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.UseCases.Roles;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IIdentityGenerator _identityGenerator;

    public RoleService(
        IRoleRepository roleRepository,
        ICurrentUser currentUser,
        IClock clock,
        IIdentityGenerator identityGenerator
    )
    {
        _roleRepository = roleRepository;
        _currentUser = currentUser;
        _clock = clock;
        _identityGenerator = identityGenerator;
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto request)
    {
        var existing = await _roleRepository.GetByCodeAsync(request.Code);
        if (existing != null)
            throw new ConflictException($"Role with code '{request.Code}' already exists");

        var role = new Role
        {
            Id = _identityGenerator.GenerateId(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
        };

        await _roleRepository.CreateAsync(role);
        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(string id, UpdateRoleDto request)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Role '{id}' not found");

        if (request.Name != null) role.Name = request.Name;
        if (request.Description != null) role.Description = request.Description;
        if (request.IsActive.HasValue) role.IsActive = request.IsActive.Value;

        role.ModifiedAtUtc = _clock.UtcNow;
        role.ModifiedBy = _currentUser.Username;

        await _roleRepository.UpdateAsync(role);
        return MapToDto(role);
    }

    public async Task DeleteRoleAsync(string id)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Role '{id}' not found");

        await _roleRepository.DeleteAsync(id);
    }

    public async Task<RoleDto> GetRoleByIdAsync(string id)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Role '{id}' not found");

        return MapToDto(role);
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        return roles.Select(MapToDto);
    }

    public async Task<PaginatedResponse<RoleDto>> SearchRolesAsync(string? search, int page, int pageSize)
    {
        var roles = await _roleRepository.SearchAsync(search, page, pageSize);
        var count = await _roleRepository.CountAsync(search);

        return new PaginatedResponse<RoleDto>
        {
            Items = roles.Select(MapToDto),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
    }

    private static RoleDto MapToDto(Role role) => new()
    {
        Id = role.Id,
        Code = role.Code,
        Name = role.Name,
        Description = role.Description,
        IsActive = role.IsActive,
    };
}
