using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Formatting;

/// <summary>
/// CONV-37: DateTime / DateTimeOffset / parseable string → UTC timestamp.
/// Unspecified DateTime is treated as UTC. Default format yyyy-MM-dd HH:mm:ss.
/// </summary>
public sealed class UtcTimestampToStringConverter : BaseDiConverter
{
    public const string DefaultFormat = "yyyy-MM-dd HH:mm:ss";

    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        DateTimeOffset stamp;
        switch (value)
        {
            case DateTimeOffset dto:
                stamp = dto;
                break;
            case DateTime dt:
                stamp = dt.Kind == DateTimeKind.Unspecified
                    ? new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc))
                    : new DateTimeOffset(dt);
                break;
            case string text when DateTimeOffset.TryParse(text, culture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed):
                stamp = parsed;
                break;
            default:
                return DependencyProperty.UnsetValue;
        }

        var format = parameter?.ToString();
        if (string.IsNullOrWhiteSpace(format) || ConversionHelpers.IsInvert(parameter))
            format = DefaultFormat;

        return stamp.UtcDateTime.ToString(format, culture);
    }
}
