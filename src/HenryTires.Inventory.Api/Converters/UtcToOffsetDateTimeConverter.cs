using System.Text.Json;
using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Api.Converters;

/// <summary>
/// JSON converter that adjusts DateTime values on serialization (write)
/// by the offset stored in HttpContext.Items["TimezoneOffsetMinutes"].
/// Deserialization (read) is unaffected — input dates stay as-is.
/// </summary>
public class UtcToOffsetDateTimeConverter : JsonConverter<DateTime>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UtcToOffsetDateTimeConverter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var adjusted = ApplyOffset(value);
        writer.WriteStringValue(adjusted.ToString("yyyy-MM-ddTHH:mm:ss"));
    }

    private DateTime ApplyOffset(DateTime value)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items["TimezoneOffsetMinutes"] is int offsetMinutes)
        {
            return value.AddMinutes(offsetMinutes);
        }
        return value;
    }
}

/// <summary>
/// Same converter for nullable DateTime.
/// </summary>
public class UtcToOffsetNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UtcToOffsetNullableDateTimeConverter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var adjusted = ApplyOffset(value.Value);
        writer.WriteStringValue(adjusted.ToString("yyyy-MM-ddTHH:mm:ss"));
    }

    private DateTime ApplyOffset(DateTime value)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items["TimezoneOffsetMinutes"] is int offsetMinutes)
        {
            return value.AddMinutes(offsetMinutes);
        }
        return value;
    }
}
