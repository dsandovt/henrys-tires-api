using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class CreateAdjustTransactionRequest
{
    public string? BranchCode { get; set; }
    public DateTime? TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public required List<AdjustTransactionLineRequest> Lines { get; set; }
}

public class AdjustTransactionLineRequest
{
    public required string ItemCode { get; set; }
    public required string Condition { get; set; }
    public required int NewQuantity { get; set; }
}

public class CommitTransactionRequest
{
    public required string TransactionId { get; set; }
}

public class CancelTransactionRequest
{
    public required string TransactionId { get; set; }
}

public class NewTransactionDto
{
    public required string Id { get; set; }
    public required string BranchCode { get; set; }
    public required string InitiatorType { get; set; }
    public required string InitiatorReference { get; set; }
    public required string InitiatorReferenceNumber { get; set; }
    public required string Status { get; set; }
    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public required List<NewTransactionLineDto> Lines { get; set; }
    public required List<StatusHistoryEntryDto> StatusHistory { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    public static NewTransactionDto FromEntity(InventoryTransaction transaction)
    {
        return new NewTransactionDto
        {
            Id = transaction.Id,
            BranchCode = transaction.BranchCode,
            InitiatorType = transaction.Initiator.EntityDefinitionCode.ToString(),
            InitiatorReference = transaction.Initiator.Reference,
            InitiatorReferenceNumber = transaction.Initiator.ReferenceNumber,
            Status = transaction.Status.ToString(),
            TransactionDateUtc = transaction.TransactionDateUtc,
            Notes = transaction.Notes,
            Lines = transaction.Lines.Select(NewTransactionLineDto.FromEntity).ToList(),
            StatusHistory = transaction.StatusHistory.Select(sh => new StatusHistoryEntryDto
            {
                Date = sh.Date,
                Status = sh.Status.ToString(),
                User = new UserLiteDto
                {
                    FirstName = sh.User.FirstName,
                    MiddleName = sh.User.MiddleName,
                    LastName = sh.User.LastName,
                    SecondLastName = sh.User.SecondLastName,
                    Username = sh.User.Username,
                    Email = sh.User.Email,
                },
                Comment = sh.Comment,
            }).ToList(),
            CreatedAtUtc = transaction.CreatedAtUtc,
            CreatedBy = transaction.CreatedBy,
            ModifiedAtUtc = transaction.ModifiedAtUtc,
            ModifiedBy = transaction.ModifiedBy,
        };
    }
}

public class NewTransactionLineDto
{
    public required string LineId { get; set; }
    public required string ItemCode { get; set; }
    public required string Condition { get; set; }
    public required int Quantity { get; set; }

    public static NewTransactionLineDto FromEntity(InventoryTransactionLine line)
    {
        return new NewTransactionLineDto
        {
            LineId = line.LineId,
            ItemCode = line.ItemCode,
            Condition = line.Condition.ToString(),
            Quantity = line.Quantity,
        };
    }
}

public class InventorySummaryDto
{
    public required string Id { get; set; }
    public required string BranchCode { get; set; }
    public required string ItemCode { get; set; }
    public required List<InventoryEntryDto> Entries { get; set; }
    public required int OnHandTotal { get; set; }
    public required int ReservedTotal { get; set; }
    public required int Version { get; set; }
    public required DateTime UpdatedAtUtc { get; set; }

    public static InventorySummaryDto FromEntity(InventorySummary summary)
    {
        return new InventorySummaryDto
        {
            Id = summary.Id,
            BranchCode = summary.BranchCode,
            ItemCode = summary.ItemCode,
            Entries = summary.Entries.Select(InventoryEntryDto.FromEntity).ToList(),
            OnHandTotal = summary.OnHandTotal,
            ReservedTotal = summary.ReservedTotal,
            Version = summary.Version,
            UpdatedAtUtc = summary.UpdatedAtUtc,
        };
    }
}

public class InventoryEntryDto
{
    public required string Condition { get; set; }
    public required int OnHand { get; set; }
    public required int Reserved { get; set; }
    public required DateTime LatestEntryDateUtc { get; set; }

    public static InventoryEntryDto FromEntity(InventoryEntry entry)
    {
        return new InventoryEntryDto
        {
            Condition = entry.Condition.ToString(),
            OnHand = entry.OnHand,
            Reserved = entry.Reserved,
            LatestEntryDateUtc = entry.LatestEntryDateUtc,
        };
    }
}

public class InventorySummaryListResponse : Common.PaginatedResponse<InventorySummaryDto>
{
    public StockTotalsDto? GeneralStock { get; set; }
}

public class StockTotalsDto
{
    public required int NewStock { get; set; }
    public required int UsedStock { get; set; }
    public required int TotalStock { get; set; }
}
