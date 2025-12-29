using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/items")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IItemManagementService _itemService;

    public ItemsController(IItemManagementService itemService)
    {
        _itemService = itemService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> CreateItem(
        [FromBody] CreateItemRequest request
    )
    {
        var result = await _itemService.CreateItemAsync(request);
        return CreatedAtAction(
            nameof(GetItemByCode),
            new { itemCode = result.ItemCode },
            ApiResponse<ItemDto>.SuccessResponse(result)
        );
    }

    [HttpPut("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> UpdateItem(
        string itemCode,
        [FromBody] UpdateItemRequest request
    )
    {
        var result = await _itemService.UpdateItemAsync(itemCode, request);
        return Ok(ApiResponse<ItemDto>.SuccessResponse(result));
    }

    [HttpDelete("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteItem(string itemCode)
    {
        await _itemService.DeleteItemAsync(itemCode);
        return Ok(ApiResponse<string>.SuccessResponse($"Item '{itemCode}' deleted successfully"));
    }

    [HttpGet("code/{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> GetItemByCode(string itemCode)
    {
        var result = await _itemService.GetItemByCodeAsync(itemCode);
        return Ok(ApiResponse<ItemDto>.SuccessResponse(result));
    }

    [HttpGet("{itemId}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> GetItemById(string itemId)
    {
        var result = await _itemService.GetItemByIdAsync(itemId);
        return Ok(ApiResponse<ItemDto>.SuccessResponse(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ItemListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemListResponse>>> SearchItems(
        [FromQuery] string? search = null,
        [FromQuery] string? classification = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var result = await _itemService.SearchItemsAsync(search, classification, page, pageSize);
        return Ok(ApiResponse<ItemListResponse>.SuccessResponse(result));
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ItemDto>>>> GetAllItems()
    {
        var result = await _itemService.GetAllItemsAsync();
        return Ok(ApiResponse<IEnumerable<ItemDto>>.SuccessResponse(result));
    }
}
