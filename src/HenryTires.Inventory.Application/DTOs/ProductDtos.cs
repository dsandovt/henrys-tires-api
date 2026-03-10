namespace HenryTires.Inventory.Application.DTOs;

public class CreateProductRequest
{
    public required string ItemCode { get; set; }
    public string? Description { get; set; }
}

public class ProductDto
{
    public required string Id { get; set; }
    public required string ItemCode { get; set; }
    public string? Description { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
}