using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Numeric;

/// <summary>CONV-14: 0–100 → GridLength star. Parameter Remainder emits (100 − value)*.</summary>
public sealed class PercentageToGridLengthConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var percent))
            return DependencyProperty.UnsetValue;

        percent = Math.Clamp(percent, 0d, 100d);
        if (IsRemainder(parameter))
            percent = 100d - percent;

        return new GridLength(percent, GridUnitType.Star);
    }

    protected override object ConvertBackCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GridLength length || length.GridUnitType != GridUnitType.Star)
            return DependencyProperty.UnsetValue;

        var percent = Math.Clamp(length.Value, 0d, 100d);
        if (IsRemainder(parameter))
            percent = 100d - percent;

        return percent;
    }

    private static bool IsRemainder(object? parameter)
        => parameter?.ToString()?.Equals("Remainder", StringComparison.OrdinalIgnoreCase) == true;
}
