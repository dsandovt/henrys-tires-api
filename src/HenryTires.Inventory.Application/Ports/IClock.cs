namespace HenryTires.Inventory.Application.Ports;

public interface IClock
{
    DateTime UtcNow { get; }
}
