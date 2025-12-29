using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request
    )
    {
        LoginResponse result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponse>.SuccessResponse(result));
    }
}
