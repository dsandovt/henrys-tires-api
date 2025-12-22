using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.DTOs;

/// <summary>
/// Item response DTO
/// </summary>
public class ItemDto
{
    public required string Id { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required string Classification { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Size { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }

    public static ItemDto FromEntity(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            Description = item.Description,
            Classification = item.Classification.ToString(),
            Category = item.Category,
            Brand = item.Brand,
            Size = item.Size,
            Notes = item.Notes,
            IsActive = item.IsActive,
            CreatedAtUtc = item.CreatedAtUtc,
            CreatedBy = item.CreatedBy,
            ModifiedAtUtc = item.ModifiedAtUtc,
            ModifiedBy = item.ModifiedBy,
            IsDeleted = item.IsDeleted
        };
    }
}

/// <summary>
/// Request to create a new item
/// </summary>
public class CreateItemRequest
{
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required string Classification { get; set; } // "Good" or "Service"
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Size { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Optional initial selling price for the item
    /// </summary>
    public decimal? InitialPrice { get; set; }

    /// <summary>
    /// Currency for the initial price (defaults to USD)
    /// </summary>
    public string? Currency { get; set; }
}

/// <summary>
/// Request to update an existing item
/// </summary>
public class UpdateItemRequest
{
    public required string Description { get; set; }
}

/// <summary>
/// Paginated list of items
/// </summary>
public class ItemListResponse
{
    public required IEnumerable<ItemDto> Items { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
