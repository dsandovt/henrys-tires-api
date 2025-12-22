using HenryTires.Inventory.Application.Ports;

namespace HenryTires.Inventory.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
