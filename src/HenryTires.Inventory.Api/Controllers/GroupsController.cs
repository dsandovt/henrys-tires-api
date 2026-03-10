using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/group")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<GroupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<GroupDto>>>> GetGroups(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null
    )
    {
        var result = await _groupService.SearchGroupsAsync(search, page, pageSize);
        return Ok(ApiResponse<PaginatedResponse<GroupDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GroupDto>>> GetGroupById(string id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        return Ok(ApiResponse<GroupDto>.SuccessResponse(group));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GroupDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<GroupDto>>> CreateGroup(
        [FromBody] CreateGroupDto request
    )
    {
        var group = await _groupService.CreateGroupAsync(request);
        return CreatedAtAction(
            nameof(GetGroupById),
            new { id = group.Id },
            ApiResponse<GroupDto>.SuccessResponse(group)
        );
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<GroupDto>>> UpdateGroup(
        string id,
        [FromBody] UpdateGroupDto request
    )
    {
        var group = await _groupService.UpdateGroupAsync(id, request);
        return Ok(ApiResponse<GroupDto>.SuccessResponse(group));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteGroup(string id)
    {
        await _groupService.DeleteGroupAsync(id);
        return Ok(
            ApiResponse<object>.SuccessResponse(new { message = "Group deleted successfully" })
        );
    }

    [HttpPatch("{id}/roles")]
    [ProducesResponseType(typeof(ApiResponse<GroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<GroupDto>>> AssignRoles(
        string id,
        [FromBody] AssignRolesRequest request
    )
    {
        var group = await _groupService.AssignRolesToGroupAsync(id, request.RoleReferences);
        return Ok(ApiResponse<GroupDto>.SuccessResponse(group));
    }

    [HttpDelete("{id}/roles")]
    [ProducesResponseType(typeof(ApiResponse<GroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GroupDto>>> RemoveRoles(
        string id,
        [FromBody] RemoveRolesRequest request
    )
    {
        var group = await _groupService.RemoveRolesFromGroupAsync(id, request.RoleReferences);
        return Ok(ApiResponse<GroupDto>.SuccessResponse(group));
    }
}
