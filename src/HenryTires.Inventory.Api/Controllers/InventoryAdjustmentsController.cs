using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/inventory-adjustments")]
[Authorize]
public class InventoryAdjustmentsController : ControllerBase
{
    private readonly IInventoryAdjustmentService _adjustmentService;

    public InventoryAdjustmentsController(IInventoryAdjustmentService adjustmentService)
    {
        _adjustmentService = adjustmentService;
    }

    [HttpPost("branch-transfer")]
    [ProducesResponseType(typeof(ApiResponse<InventoryAdjustmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> CreateBranchTransfer(
        [FromBody] CreateBranchTransferDto request
    )
    {
        var result = await _adjustmentService.CreateBranchTransferAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<InventoryAdjustmentDto>.SuccessResponse(result)
        );
    }

    [HttpPost("stock-correction")]
    [ProducesResponseType(typeof(ApiResponse<InventoryAdjustmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> CreateStockCorrection(
        [FromBody] CreateStockCorrectionDto request
    )
    {
        var result = await _adjustmentService.CreateStockCorrectionAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<InventoryAdjustmentDto>.SuccessResponse(result)
        );
    }

    [HttpPost("{id}/commit")]
    [ProducesResponseType(typeof(ApiResponse<InventoryAdjustmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> CommitAdjustment(string id)
    {
        var result = await _adjustmentService.CommitAdjustmentAsync(id);
        return Ok(ApiResponse<InventoryAdjustmentDto>.SuccessResponse(result));
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<InventoryAdjustmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> CancelAdjustment(string id)
    {
        var result = await _adjustmentService.CancelAdjustmentAsync(id);
        return Ok(ApiResponse<InventoryAdjustmentDto>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryAdjustmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> GetById(string id)
    {
        var result = await _adjustmentService.GetByIdAsync(id);
        return Ok(ApiResponse<InventoryAdjustmentDto>.SuccessResponse(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<InventoryAdjustmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<InventoryAdjustmentDto>>>> Search(
        [FromQuery] string? branchReference = null,
        [FromQuery] AdjustmentType? adjustmentType = null,
        [FromQuery] InventoryAdjustmentStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var result = await _adjustmentService.SearchAsync(
            branchReference,
            adjustmentType,
            status,
            from,
            to,
            page,
            pageSize
        );
        return Ok(ApiResponse<PaginatedResponse<InventoryAdjustmentDto>>.SuccessResponse(result));
    }
}
