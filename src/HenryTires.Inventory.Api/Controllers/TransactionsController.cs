using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

/// <summary>
/// Transaction management endpoints
/// </summary>
[ApiController]
[Route("api/v1/transaction")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly INewTransactionService _transactionService;
    private readonly ICurrentUserService _currentUser;

    public TransactionsController(
        INewTransactionService transactionService,
        ICurrentUserService currentUser
    )
    {
        _transactionService = transactionService;
        _currentUser = currentUser;
    }

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

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<NewTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<PaginatedResponse<NewTransactionDto>>>
    > GetTransactionsByBranch(
        [FromQuery] string? branchReference = null,
        [FromQuery] string? initiatorType = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        InitiatorType? parsedInitiatorType = null;
        if (
            !string.IsNullOrWhiteSpace(initiatorType)
            && Enum.TryParse<InitiatorType>(initiatorType, true, out var parsedType)
        )
        {
            parsedInitiatorType = parsedType;
        }

        InventoryTransactionStatus? transactionStatus = null;
        if (
            !string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<InventoryTransactionStatus>(status, true, out var parsedStatus)
        )
        {
            transactionStatus = parsedStatus;
        }

        var result = await _transactionService.GetTransactionsByBranchAsync(
            branchReference,
            parsedInitiatorType,
            transactionStatus,
            page,
            pageSize
        );

        return Ok(ApiResponse<PaginatedResponse<NewTransactionDto>>.SuccessResponse(result));
    }

    [HttpGet("inventory-summary")]
    [ProducesResponseType(typeof(ApiResponse<InventorySummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InventorySummaryDto?>>> GetInventorySummary(
        [FromQuery] string itemCode,
        [FromQuery] string? branchReference = null
    )
    {
        // StoreSeller users can only view inventory from their assigned branches
        if (_currentUser.RoleCodes != null && !_currentUser.RoleCodes.Contains("ADMIN"))
        {
            var branchRefs = _currentUser.BranchReferences;
            if (branchRefs == null || branchRefs.Count == 0)
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse("User must have a branch assigned")
                );
            }
            if (!string.IsNullOrEmpty(branchReference) && !branchRefs.Contains(branchReference))
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse(
                        "Access denied: you do not have access to the specified branch"
                    )
                );
            }
            if (string.IsNullOrEmpty(branchReference))
            {
                branchReference = branchRefs[0];
            }
        }

        var result = await _transactionService.GetInventorySummaryAsync(branchReference, itemCode);
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

    [HttpGet("inventory")]
    [ProducesResponseType(
        typeof(ApiResponse<InventorySummaryListResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<
        ActionResult<ApiResponse<InventorySummaryListResponse>>
    > GetInventorySummariesByBranch(
        [FromQuery] string? branchReference = null,
        [FromQuery] string? search = null,
        [FromQuery] string? condition = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        // Non-admin users can only view inventory from their assigned branches
        if (_currentUser.RoleCodes != null && !_currentUser.RoleCodes.Contains("ADMIN"))
        {
            var branchRefs = _currentUser.BranchReferences;
            if (branchRefs == null || branchRefs.Count == 0)
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse("User must have a branch assigned")
                );
            }
            if (!string.IsNullOrEmpty(branchReference) && !branchRefs.Contains(branchReference))
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse(
                        "Access denied: you do not have access to the specified branch"
                    )
                );
            }
            if (string.IsNullOrEmpty(branchReference))
            {
                branchReference = branchRefs[0];
            }
        }

        ItemCondition? conditionEnum = null;
        if (
            !string.IsNullOrWhiteSpace(condition)
            && Enum.TryParse<ItemCondition>(condition, true, out var parsedCondition)
        )
        {
            conditionEnum = parsedCondition;
        }

        var result = await _transactionService.GetInventorySummariesByBranchAsync(
            branchReference,
            search,
            conditionEnum,
            page,
            pageSize
        );

        return Ok(ApiResponse<InventorySummaryListResponse>.SuccessResponse(result));
    }
}
