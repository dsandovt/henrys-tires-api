namespace HenryTires.Inventory.Domain.ValueObjects;

public class StatusHistoryEntry<T>
{
    public required DateTime Date { get; set; }
    public required T Status { get; set; }
    public required UserLite User { get; set; }
    public string? Comment { get; set; }
}
