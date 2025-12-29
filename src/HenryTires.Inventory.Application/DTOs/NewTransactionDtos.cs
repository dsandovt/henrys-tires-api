using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class CreateInTransactionRequest
{
    public string? BranchCode { get; set; } // Optional for Admin users
    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public required List<InTransactionLineRequest> Lines { get; set; }
}

public class InTransactionLineRequest
{
    public required string ItemCode { get; set; }
    public required string ItemCondition { get; set; } // "New" or "Used"
    public required int Quantity { get; set; }
    public decimal? UnitPrice { get; set; } // Optional - will use ConsumableItemPrice if not provided
    public Currency? Currency { get; set; } // Optional - defaults to USD
    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;
    public string? PriceNotes { get; set; } // Optional - notes about the price
}

public class CreateOutTransactionRequest
{
    public string? BranchCode { get; set; } // Optional for Admin users
    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public required List<OutTransactionLineRequest> Lines { get; set; }
}

public class OutTransactionLineRequest
{
    public required string ItemCode { get; set; }
    public required string ItemCondition { get; set; } // "New" or "Used"
    public required int Quantity { get; set; }
    public decimal? UnitPrice { get; set; } // Optional - Admin/Supervisor can override selling price
    public Currency? Currency { get; set; } // Optional - defaults to USD
    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;
    public string? PriceNotes { get; set; } // Optional - reason for price override (e.g., "Manager discount")
}

public class CreateAdjustTransactionRequest
{
    public string? BranchCode { get; set; } // Optional for Admin users
    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public required List<AdjustTransactionLineRequest> Lines { get; set; }
}

public class AdjustTransactionLineRequest
{
    public required string ItemCode { get; set; }
    public required string ItemCondition { get; set; } // "New" or "Used"
    public required int NewQuantity { get; set; } // The new absolute quantity (not delta)
    public decimal? UnitPrice { get; set; } // Optional - will use ConsumableItemPrice if not provided
    public Currency? Currency { get; set; } // Optional - defaults to USD
    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;
    public string? PriceNotes { get; set; } // Optional - notes about the price
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
    public required string TransactionNumber { get; set; }
    public required string BranchCode { get; set; }
    public required string Type { get; set; } // In, Out, Adjust
    public required string Status { get; set; } // Draft, Committed, Cancelled
    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? CommittedAtUtc { get; set; }
    public string? CommittedBy { get; set; }
    public required List<NewTransactionLineDto> Lines { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    public static NewTransactionDto FromEntity(InventoryTransaction transaction)
    {
        return new NewTransactionDto
        {
            Id = transaction.Id,
            TransactionNumber = transaction.TransactionNumber,
            BranchCode = transaction.BranchCode,
            Type = transaction.Type.ToString(),
            Status = transaction.Status.ToString(),
            TransactionDateUtc = transaction.TransactionDateUtc,
            Notes = transaction.Notes,
            PaymentMethod = transaction.PaymentMethod,
            CommittedAtUtc = transaction.CommittedAtUtc,
            CommittedBy = transaction.CommittedBy,
            Lines = transaction.Lines.Select(NewTransactionLineDto.FromEntity).ToList(),
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
    public required string ItemCondition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required Currency Currency { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;
    public required string PriceSource { get; set; } // ConsumableItemPrice, Manual, Sale, SystemDefault, PurchaseOrder, AverageCost
    public required string PriceSetByRole { get; set; } // Role that set the price (Admin, Supervisor, Seller, System)
    public required string PriceSetByUser { get; set; } // User who set the price
    public required decimal LineTotal { get; set; }
    public decimal? CostOfGoodsSold { get; set; } // Optional: COGS for profit calculations
    public string? PriceNotes { get; set; } // Optional: Notes about the price
    public required DateTime ExecutedAtUtc { get; set; }

    public static NewTransactionLineDto FromEntity(InventoryTransactionLine line)
    {
        return new NewTransactionLineDto
        {
            LineId = line.LineId,
            ItemCode = line.ItemCode,
            ItemCondition = line.Condition.ToString(),
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            Currency = line.Currency,
            IsTaxable = line.IsTaxable,
            AppliesShopFee = line.AppliesShopFee,
            PriceSource = line.PriceSource.ToString(),
            PriceSetByRole = line.PriceSetByRole,
            PriceSetByUser = line.PriceSetByUser,
            LineTotal = line.LineTotal,
            CostOfGoodsSold = line.CostOfGoodsSold,
            PriceNotes = line.PriceNotes,
            ExecutedAtUtc = line.ExecutedAtUtc,
        };
    }
}

public class NewTransactionListResponse
{
    public required IEnumerable<NewTransactionDto> Items { get; set; }
    public required long TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
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
    public required string ItemCondition { get; set; }
    public required int OnHand { get; set; }
    public required int Reserved { get; set; }
    public required DateTime LatestEntryDateUtc { get; set; }

    public static InventoryEntryDto FromEntity(InventoryEntry entry)
    {
        return new InventoryEntryDto
        {
            ItemCondition = entry.Condition.ToString(),
            OnHand = entry.OnHand,
            Reserved = entry.Reserved,
            LatestEntryDateUtc = entry.LatestEntryDateUtc,
        };
    }
}

public class InventorySummaryListResponse
{
    public required IEnumerable<InventorySummaryDto> Items { get; set; }
    public required long TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public StockTotalsDto? GeneralStock { get; set; }
}

public class StockTotalsDto
{
    public required int NewStock { get; set; }
    public required int UsedStock { get; set; }
    public required int TotalStock { get; set; }
}
