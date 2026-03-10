namespace HenryTires.Inventory.Api.Helpers;

public static class TimezoneHelper
{
    public static DateTime LocalToUtc(DateTime localDate, int offsetMinutes)
        => localDate.AddMinutes(-offsetMinutes);

    public static DateTime? LocalToUtc(DateTime? localDate, int offsetMinutes)
        => localDate?.AddMinutes(-offsetMinutes);

    public static DateTime LocalStartOfDayToUtc(DateTime localDate, int offsetMinutes)
        => localDate.Date.AddMinutes(-offsetMinutes);

    public static DateTime LocalEndOfDayToUtc(DateTime localDate, int offsetMinutes)
        => localDate.Date.AddDays(1).AddMinutes(-offsetMinutes).AddTicks(-1);
}
