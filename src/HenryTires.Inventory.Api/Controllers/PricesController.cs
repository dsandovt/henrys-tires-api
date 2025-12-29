using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/prices")]
[Authorize]
public class PricesController : ControllerBase
{
    private readonly IPriceManagementService _priceService;

    public PricesController(IPriceManagementService priceService)
    {
        _priceService = priceService;
    }

    [HttpPut("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<ConsumableItemPriceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ConsumableItemPriceDto>>> UpdateItemPrice(
        string itemCode,
        [FromBody] UpdateItemPriceRequest request
    )
    {
        var result = await _priceService.UpdateItemPriceAsync(itemCode, request);
        return Ok(ApiResponse<ConsumableItemPriceDto>.SuccessResponse(result));
    }

    [HttpGet("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<ConsumableItemPriceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ConsumableItemPriceDto?>>> GetItemPrice(
        string itemCode
    )
    {
        var result = await _priceService.GetItemPriceAsync(itemCode);
        if (result == null)
        {
            return NotFound(
                ApiResponse<ConsumableItemPriceDto>.ErrorResponse(
                    $"Price for item '{itemCode}' not found"
                )
            );
        }
        return Ok(ApiResponse<ConsumableItemPriceDto>.SuccessResponse(result));
    }

    [HttpGet("{itemCode}/history")]
    [ProducesResponseType(
        typeof(ApiResponse<ConsumableItemPriceWithHistoryDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<ApiResponse<ConsumableItemPriceWithHistoryDto?>>
    > GetItemPriceWithHistory(string itemCode)
    {
        var result = await _priceService.GetItemPriceWithHistoryAsync(itemCode);
        if (result == null)
        {
            return NotFound(
                ApiResponse<ConsumableItemPriceWithHistoryDto>.ErrorResponse(
                    $"Price for item '{itemCode}' not found"
                )
            );
        }
        return Ok(ApiResponse<ConsumableItemPriceWithHistoryDto>.SuccessResponse(result));
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<ConsumableItemPriceDto>>),
        StatusCodes.Status200OK
    )]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<ConsumableItemPriceDto>>>
    > GetAllItemPrices()
    {
        var result = await _priceService.GetAllItemPricesAsync();
        return Ok(ApiResponse<IEnumerable<ConsumableItemPriceDto>>.SuccessResponse(result));
    }
}
