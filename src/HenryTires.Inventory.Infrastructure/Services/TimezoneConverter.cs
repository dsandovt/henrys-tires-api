using HenryTires.Inventory.Application.Common;
using Microsoft.Extensions.Configuration;

namespace HenryTires.Inventory.Infrastructure.Services;

public class TimezoneConverter : ITimezoneConverter
{
    private readonly TimeZoneInfo _timeZone;
    private readonly string _timezoneId;

    public TimezoneConverter(IConfiguration configuration)
    {
        _timezoneId = configuration["AppSettings:Timezone"] ?? "America/New_York";
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(_timezoneId);
    }

    public DateTime ConvertUtcToEastern(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _timeZone);
    }

    public string GetTimezoneAbbreviation(DateTime utcDateTime)
    {
        var localTime = ConvertUtcToEastern(utcDateTime);
        return _timeZone.IsDaylightSavingTime(localTime) ? "EDT" : "EST";
    }

    public string GetTimezoneId() => _timezoneId;
}
