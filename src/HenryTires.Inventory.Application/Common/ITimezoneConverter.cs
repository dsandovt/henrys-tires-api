namespace HenryTires.Inventory.Application.Common;

/// <summary>
/// Interface for converting UTC dates to local timezone (Eastern Time - Newport News, VA)
/// </summary>
public interface ITimezoneConverter
{
    /// <summary>
    /// Convert UTC DateTime to Eastern Time
    /// </summary>
    DateTime ConvertUtcToEastern(DateTime utcDateTime);

    /// <summary>
    /// Get the timezone abbreviation (EST/EDT) for display
    /// </summary>
    string GetTimezoneAbbreviation(DateTime utcDateTime);
}
