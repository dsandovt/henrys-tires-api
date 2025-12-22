using HenryTires.Inventory.Application.Common;

namespace HenryTires.Inventory.Infrastructure.Services;

/// <summary>
/// Converts UTC dates to Eastern Time (Newport News, VA timezone)
/// </summary>
public class TimezoneConverter : ITimezoneConverter
{
    private static readonly TimeZoneInfo EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    public DateTime ConvertUtcToEastern(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, EasternTimeZone);
    }

    public string GetTimezoneAbbreviation(DateTime utcDateTime)
    {
        var easternTime = ConvertUtcToEastern(utcDateTime);
        return EasternTimeZone.IsDaylightSavingTime(easternTime) ? "EDT" : "EST";
    }
}
