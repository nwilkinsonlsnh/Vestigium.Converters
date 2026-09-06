using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Formatting;

/// <summary>CONV-29: milliseconds → "42 ms". TimeSpan uses TotalMilliseconds.</summary>
public sealed class MillisecondsToStringConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double ms;
        if (value is TimeSpan span)
            ms = span.TotalMilliseconds;
        else if (!ConversionHelpers.TryToDouble(value, culture, out ms))
            return DependencyProperty.UnsetValue;

        var decimals = 0;
        if (parameter is not null && ConversionHelpers.TryToInt(parameter, culture, out var parsed))
            decimals = Math.Clamp(parsed, 0, 6);

        var format = "F" + decimals.ToString(CultureInfo.InvariantCulture);
        return string.Concat(ms.ToString(format, culture), " ms");
    }
}
