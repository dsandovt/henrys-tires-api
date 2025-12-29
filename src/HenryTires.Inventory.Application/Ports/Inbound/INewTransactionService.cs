using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface INewTransactionService
{
    Task<NewTransactionDto> CreateInTransactionAsync(CreateInTransactionRequest request);
    Task<NewTransactionDto> CreateOutTransactionAsync(CreateOutTransactionRequest request);
    Task<NewTransactionDto> CreateAdjustTransactionAsync(CreateAdjustTransactionRequest request);
    Task<NewTransactionDto> CommitTransactionAsync(CommitTransactionRequest request);
    Task<NewTransactionDto> CancelTransactionAsync(CancelTransactionRequest request);
    Task<NewTransactionDto> GetTransactionByIdAsync(string transactionId);
    Task<NewTransactionListResponse> GetTransactionsByBranchAsync(
        string? branchCode,
        TransactionType? type,
        TransactionStatus? status,
        int page,
        int pageSize
    );
    Task<InventorySummaryDto?> GetInventorySummaryAsync(string? branchCode, string itemCode);
    Task<InventorySummaryListResponse> GetInventorySummariesByBranchAsync(
        string? branchCode,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    );
}
