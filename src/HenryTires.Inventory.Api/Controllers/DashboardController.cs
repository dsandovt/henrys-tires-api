using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.UseCases.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get dashboard data with pre-aggregated metrics
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DashboardDataDto>>> GetDashboardData(
        [FromQuery] DateTime startDateUtc,
        [FromQuery] DateTime endDateUtc,
        [FromQuery] string? branchCode = null)
    {
        if (endDateUtc < startDateUtc)
        {
            return BadRequest(ApiResponse<DashboardDataDto>.ErrorResponse("End date must be after start date"));
        }

        var data = await _dashboardService.GetDashboardDataAsync(startDateUtc, endDateUtc, branchCode);
        return Ok(ApiResponse<DashboardDataDto>.SuccessResponse(data));
    }
}
