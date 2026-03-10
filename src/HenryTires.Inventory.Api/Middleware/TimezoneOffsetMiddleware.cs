namespace HenryTires.Inventory.Api.Middleware;

/// <summary>
/// Reads X-Timezone-Offset header (minutes, e.g. -300 for EST)
/// and stores it in HttpContext.Items so the JSON converter can use it.
/// </summary>
public class TimezoneOffsetMiddleware
{
    private readonly RequestDelegate _next;

    public TimezoneOffsetMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Timezone-Offset", out var offsetHeader)
            && int.TryParse(offsetHeader, out var offsetMinutes))
        {
            context.Items["TimezoneOffsetMinutes"] = offsetMinutes;
        }

        await _next(context);
    }
}
