namespace HenryTires.Inventory.Application.Common;

public class PaginatedResponse<T>
{
    public required IEnumerable<T> Items { get; set; }
    public required long TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
