using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.UseCases.Groups;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IIdentityGenerator _identityGenerator;

    public GroupService(
        IGroupRepository groupRepository,
        IRoleRepository roleRepository,
        ICurrentUser currentUser,
        IClock clock,
        IIdentityGenerator identityGenerator
    )
    {
        _groupRepository = groupRepository;
        _roleRepository = roleRepository;
        _currentUser = currentUser;
        _clock = clock;
        _identityGenerator = identityGenerator;
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupDto request)
    {
        var existing = await _groupRepository.GetByCodeAsync(request.Code);
        if (existing != null)
            throw new ConflictException($"Group with code '{request.Code}' already exists");

        // Validate all role IDs exist
        foreach (var roleId in request.RoleReferences)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                throw new ValidationException($"Role '{roleId}' not found");
        }

        var group = new Group
        {
            Id = _identityGenerator.GenerateId(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            RoleReferences = request.RoleReferences,
            IsActive = request.IsActive,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
        };

        await _groupRepository.CreateAsync(group);
        return MapToDto(group);
    }

    public async Task<GroupDto> UpdateGroupAsync(string id, UpdateGroupDto request)
    {
        var group = await _groupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Group '{id}' not found");

        if (request.Name != null) group.Name = request.Name;
        if (request.Description != null) group.Description = request.Description;
        if (request.IsActive.HasValue) group.IsActive = request.IsActive.Value;

        if (request.RoleReferences != null)
        {
            foreach (var roleId in request.RoleReferences)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                    throw new ValidationException($"Role '{roleId}' not found");
            }
            group.RoleReferences = request.RoleReferences;
        }

        group.ModifiedAtUtc = _clock.UtcNow;
        group.ModifiedBy = _currentUser.Username;

        await _groupRepository.UpdateAsync(group);
        return MapToDto(group);
    }

    public async Task DeleteGroupAsync(string id)
    {
        var group = await _groupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Group '{id}' not found");

        await _groupRepository.DeleteAsync(id);
    }

    public async Task<GroupDto> GetGroupByIdAsync(string id)
    {
        var group = await _groupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Group '{id}' not found");

        return MapToDto(group);
    }

    public async Task<IEnumerable<GroupDto>> GetAllGroupsAsync()
    {
        var groups = await _groupRepository.GetAllAsync();
        return groups.Select(MapToDto);
    }

    public async Task<PaginatedResponse<GroupDto>> SearchGroupsAsync(string? search, int page, int pageSize)
    {
        var groups = await _groupRepository.SearchAsync(search, page, pageSize);
        var count = await _groupRepository.CountAsync(search);

        return new PaginatedResponse<GroupDto>
        {
            Items = groups.Select(MapToDto),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<GroupDto> AssignRolesToGroupAsync(string groupId, List<string> roleIds)
    {
        var group = await _groupRepository.GetByIdAsync(groupId)
            ?? throw new NotFoundException($"Group '{groupId}' not found");

        foreach (var roleId in roleIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                throw new ValidationException($"Role '{roleId}' not found");

            if (!group.RoleReferences.Contains(roleId))
                group.RoleReferences.Add(roleId);
        }

        group.ModifiedAtUtc = _clock.UtcNow;
        group.ModifiedBy = _currentUser.Username;

        await _groupRepository.UpdateAsync(group);
        return MapToDto(group);
    }

    public async Task<GroupDto> RemoveRolesFromGroupAsync(string groupId, List<string> roleIds)
    {
        var group = await _groupRepository.GetByIdAsync(groupId)
            ?? throw new NotFoundException($"Group '{groupId}' not found");

        group.RoleReferences = group.RoleReferences.Where(r => !roleIds.Contains(r)).ToList();

        group.ModifiedAtUtc = _clock.UtcNow;
        group.ModifiedBy = _currentUser.Username;

        await _groupRepository.UpdateAsync(group);
        return MapToDto(group);
    }

    private static GroupDto MapToDto(Group group) => new()
    {
        Id = group.Id,
        Code = group.Code,
        Name = group.Name,
        Description = group.Description,
        RoleReferences = group.RoleReferences,
        IsActive = group.IsActive,
    };
}
