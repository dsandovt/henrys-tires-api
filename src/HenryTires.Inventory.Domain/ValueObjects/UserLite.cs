namespace HenryTires.Inventory.Domain.ValueObjects;

public class UserLite
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? SecondLastName { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
}
