using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/role")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<RoleDto>>>> GetRoles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null
    )
    {
        var result = await _roleService.SearchRolesAsync(search, page, pageSize);
        return Ok(ApiResponse<PaginatedResponse<RoleDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(string id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(role));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole(
        [FromBody] CreateRoleDto request
    )
    {
        var role = await _roleService.CreateRoleAsync(request);
        return CreatedAtAction(
            nameof(GetRoleById),
            new { id = role.Id },
            ApiResponse<RoleDto>.SuccessResponse(role)
        );
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRole(
        string id,
        [FromBody] UpdateRoleDto request
    )
    {
        var role = await _roleService.UpdateRoleAsync(id, request);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(role));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRole(string id)
    {
        await _roleService.DeleteRoleAsync(id);
        return Ok(
            ApiResponse<object>.SuccessResponse(new { message = "Role deleted successfully" })
        );
    }
}
