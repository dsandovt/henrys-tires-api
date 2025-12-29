using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users with optional search and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<UserListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null
    )
    {
        var result = await _userService.GetUsersAsync(page, pageSize, search);
        return Ok(ApiResponse<UserListResponse>.SuccessResponse(result));
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(string id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
        [FromBody] CreateUserRequest request
    )
    {
        var result = await _userService.CreateUserAsync(request);
        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Id },
            ApiResponse<UserDto>.SuccessResponse(result)
        );
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
        string id,
        [FromBody] UpdateUserRequest request
    )
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(string id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(
            ApiResponse<object>.SuccessResponse(new { message = "User deleted successfully" })
        );
    }

    /// <summary>
    /// Toggle user active status
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> ToggleUserStatus(string id)
    {
        var result = await _userService.ToggleUserStatusAsync(id);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }
}
