namespace HenryTires.Inventory.Application.DTOs;

public class CreateTransactionRequest
{
    public string? BranchId { get; set; } // Optional for Admin users
    public required string Type { get; set; } // "In" or "Out"
    public string? Notes { get; set; }
    public required List<TransactionLineRequest> Lines { get; set; }
}

public class TransactionLineRequest
{
    public required string ProductId { get; set; }
    public required string ItemCondition { get; set; } // "New" or "Used"
    public required int Quantity { get; set; }
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
}

public class TransactionDto
{
    public required string Id { get; set; }
    public required string BranchId { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? PostedAtUtc { get; set; }
    public required List<TransactionLineDto> Lines { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
}

public class TransactionLineDto
{
    public required string LineId { get; set; }
    public required string ProductId { get; set; }
    public required string ItemCondition { get; set; }
    public required int Quantity { get; set; }
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public required DateTime ExecutedAtUtc { get; set; }
}

public class TransactionListResponse
{
    public required IEnumerable<TransactionDto> Items { get; set; }
    public required long TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}

public class StockBalanceDto
{
    public required string ProductId { get; set; }
    public required string ProductCode { get; set; }
    public string? ProductDescription { get; set; }
    public required string ItemCondition { get; set; }
    public required int QuantityOnHand { get; set; }
    public required DateTime UpdatedAtUtc { get; set; }
}

public class StockListResponse
{
    public required IEnumerable<StockBalanceDto> Items { get; set; }
    public required long TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
