using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

/// <summary>
/// Transaction management endpoints
/// </summary>
[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly INewTransactionService _transactionService;

    public TransactionsController(INewTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Create an IN transaction (draft)
    /// </summary>
    [HttpPost("in")]
    [ProducesResponseType(typeof(ApiResponse<NewTransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NewTransactionDto>>> CreateInTransaction(
        [FromBody] CreateInTransactionRequest request
    )
    {
        var result = await _transactionService.CreateInTransactionAsync(request);
        return CreatedAtAction(
            nameof(GetTransactionById),
            new { transactionId = result.Id },
            ApiResponse<NewTransactionDto>.SuccessResponse(result)
        );
    }

    /// <summary>
    /// Create an OUT transaction (draft) - validates stock availability
    /// </summary>
    [HttpPost("out")]
    [ProducesResponseType(typeof(ApiResponse<NewTransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NewTransactionDto>>> CreateOutTransaction(
        [FromBody] CreateOutTransactionRequest request
    )
    {
        var result = await _transactionService.CreateOutTransactionAsync(request);
        return CreatedAtAction(
            nameof(GetTransactionById),
            new { transactionId = result.Id },
            ApiResponse<NewTransactionDto>.SuccessResponse(result)
        );
    }

    /// <summary>
    /// Create an ADJUST transaction (draft)
    /// </summary>
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(ApiResponse<NewTransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NewTransactionDto>>> CreateAdjustTransaction(
        [FromBody] CreateAdjustTransactionRequest request
    )
    {
        var result = await _transactionService.CreateAdjustTransactionAsync(request);
        return CreatedAtAction(
            nameof(GetTransactionById),
            new { transactionId = result.Id },
            ApiResponse<NewTransactionDto>.SuccessResponse(result)
        );
    }

    /// <summary>
    /// Commit a draft transaction - atomically updates transaction and inventory summary
    /// </summary>
    [HttpPost("{transactionId}/commit")]
    [ProducesResponseType(typeof(ApiResponse<NewTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<NewTransactionDto>>> CommitTransaction(
        string transactionId
    )
    {
        var request = new CommitTransactionRequest { TransactionId = transactionId };
        var result = await _transactionService.CommitTransactionAsync(request);
        return Ok(ApiResponse<NewTransactionDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Cancel a draft transaction
    /// </summary>
    [HttpPost("{transactionId}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<NewTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NewTransactionDto>>> CancelTransaction(
        string transactionId
    )
    {
        var request = new CancelTransactionRequest { TransactionId = transactionId };
        var result = await _transactionService.CancelTransactionAsync(request);
        return Ok(ApiResponse<NewTransactionDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{transactionId}")]
    [ProducesResponseType(typeof(ApiResponse<NewTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NewTransactionDto>>> GetTransactionById(
        string transactionId
    )
    {
        var result = await _transactionService.GetTransactionByIdAsync(transactionId);
        return Ok(ApiResponse<NewTransactionDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get paginated list of transactions for a branch
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<NewTransactionListResponse>), StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<NewTransactionListResponse>>
    > GetTransactionsByBranch(
        [FromQuery] string? branchCode = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        TransactionType? transactionType = null;
        if (
            !string.IsNullOrWhiteSpace(type)
            && Enum.TryParse<TransactionType>(type, true, out var parsedType)
        )
        {
            transactionType = parsedType;
        }

        TransactionStatus? transactionStatus = null;
        if (
            !string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<TransactionStatus>(status, true, out var parsedStatus)
        )
        {
            transactionStatus = parsedStatus;
        }

        var result = await _transactionService.GetTransactionsByBranchAsync(
            branchCode,
            transactionType,
            transactionStatus,
            page,
            pageSize
        );

        return Ok(ApiResponse<NewTransactionListResponse>.SuccessResponse(result));
    }

    /// <summary>
    /// Get inventory summary for an item at a branch
    /// </summary>
    [HttpGet("inventory/{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<InventorySummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InventorySummaryDto?>>> GetInventorySummary(
        string itemCode,
        [FromQuery] string? branchCode = null
    )
    {
        var result = await _transactionService.GetInventorySummaryAsync(branchCode, itemCode);
        if (result == null)
        {
            return NotFound(
                ApiResponse<InventorySummaryDto>.ErrorResponse(
                    $"Inventory summary not found for item '{itemCode}'"
                )
            );
        }
        return Ok(ApiResponse<InventorySummaryDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get paginated inventory summaries for a branch
    /// </summary>
    [HttpGet("inventory")]
    [ProducesResponseType(
        typeof(ApiResponse<InventorySummaryListResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<
        ActionResult<ApiResponse<InventorySummaryListResponse>>
    > GetInventorySummariesByBranch(
        [FromQuery] string? branchCode = null,
        [FromQuery] string? search = null,
        [FromQuery] string? condition = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        ItemCondition? conditionEnum = null;
        if (
            !string.IsNullOrWhiteSpace(condition)
            && Enum.TryParse<ItemCondition>(condition, true, out var parsedCondition)
        )
        {
            conditionEnum = parsedCondition;
        }

        var result = await _transactionService.GetInventorySummariesByBranchAsync(
            branchCode,
            search,
            conditionEnum,
            page,
            pageSize
        );

        return Ok(ApiResponse<InventorySummaryListResponse>.SuccessResponse(result));
    }
}
