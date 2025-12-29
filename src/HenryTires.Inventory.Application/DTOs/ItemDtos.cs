using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class ItemDto
{
    public required string Id { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required string Classification { get; set; }
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
            Notes = item.Notes,
            IsActive = item.IsActive,
            CreatedAtUtc = item.CreatedAtUtc,
            CreatedBy = item.CreatedBy,
            ModifiedAtUtc = item.ModifiedAtUtc,
            ModifiedBy = item.ModifiedBy,
            IsDeleted = item.IsDeleted,
        };
    }
}

public class CreateItemRequest
{
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required string Classification { get; set; } // "Good" or "Service"
    public string? Notes { get; set; }

    public decimal? InitialPrice { get; set; }
    public Currency? Currency { get; set; }
}

public class UpdateItemRequest
{
    public required string Description { get; set; }
}

public class ItemListResponse
{
    public required IEnumerable<ItemDto> Items { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
