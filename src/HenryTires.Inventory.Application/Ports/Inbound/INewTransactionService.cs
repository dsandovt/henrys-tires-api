using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface INewTransactionService
{
    Task<NewTransactionDto> CreateAdjustTransactionAsync(CreateAdjustTransactionRequest request);
    Task<NewTransactionDto> CommitTransactionAsync(CommitTransactionRequest request);
    Task<NewTransactionDto> CancelTransactionAsync(CancelTransactionRequest request);
    Task<NewTransactionDto> GetTransactionByIdAsync(string transactionId);
    Task<PaginatedResponse<NewTransactionDto>> GetTransactionsByBranchAsync(
        string? branchReference,
        InitiatorType? initiatorType,
        InventoryTransactionStatus? status,
        int page,
        int pageSize
    );
    Task<InventorySummaryDto?> GetInventorySummaryAsync(string? branchReference, string itemCode);
    Task<InventorySummaryListResponse> GetInventorySummariesByBranchAsync(
        string? branchReference,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    );
}
