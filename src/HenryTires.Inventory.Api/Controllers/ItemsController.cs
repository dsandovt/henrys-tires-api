using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.UseCases.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

/// <summary>
/// Item management endpoints (master data)
/// </summary>
[ApiController]
[Route("api/v1/items")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly ItemManagementService _itemService;

    public ItemsController(ItemManagementService itemService)
    {
        _itemService = itemService;
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> CreateItem(
        [FromBody] CreateItemRequest request)
    {
        var result = await _itemService.CreateItemAsync(request);
        return CreatedAtAction(
            nameof(GetItemByCode),
            new { itemCode = result.ItemCode },
            ApiResponse<ItemDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Update an existing item
    /// </summary>
    [HttpPut("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> UpdateItem(
        string itemCode,
        [FromBody] UpdateItemRequest request)
    {
        var result = await _itemService.UpdateItemAsync(itemCode, request);
        return Ok(ApiResponse<ItemDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Soft delete an item
    /// </summary>
    [HttpDelete("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteItem(string itemCode)
    {
        await _itemService.DeleteItemAsync(itemCode);
        return Ok(ApiResponse<string>.SuccessResponse($"Item '{itemCode}' deleted successfully"));
    }

    /// <summary>
    /// Get item by code
    /// </summary>
    [HttpGet("code/{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> GetItemByCode(string itemCode)
    {
        var result = await _itemService.GetItemByCodeAsync(itemCode);
        return Ok(ApiResponse<ItemDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get item by ID
    /// </summary>
    [HttpGet("{itemId}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> GetItemById(string itemId)
    {
        var result = await _itemService.GetItemByIdAsync(itemId);
        return Ok(ApiResponse<ItemDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Search items with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ItemListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemListResponse>>> SearchItems(
        [FromQuery] string? search = null,
        [FromQuery] string? classification = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _itemService.SearchItemsAsync(search, classification, page, pageSize);
        return Ok(ApiResponse<ItemListResponse>.SuccessResponse(result));
    }

    /// <summary>
    /// Get all items (for dropdown lists)
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ItemDto>>>> GetAllItems()
    {
        var result = await _itemService.GetAllItemsAsync();
        return Ok(ApiResponse<IEnumerable<ItemDto>>.SuccessResponse(result));
    }
}
