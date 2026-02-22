namespace HenryTires.Inventory.Domain.Entities;

public class Branch
{
    public required string Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
}
