namespace HenryTires.Inventory.Application.Common;

public interface ITimezoneConverter
{
    DateTime ConvertUtcToEastern(DateTime utcDateTime);
    string GetTimezoneAbbreviation(DateTime utcDateTime);
}
