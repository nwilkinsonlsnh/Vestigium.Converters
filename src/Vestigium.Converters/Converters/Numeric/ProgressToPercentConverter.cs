using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Numeric;

/// <summary>
/// CONV-39: 0–1 progress → 0–100. Values already &gt; 1 are treated as percent and clamped.
/// </summary>
public sealed class ProgressToPercentConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var number))
            return DependencyProperty.UnsetValue;

        if (number <= 1d)
            number *= 100d;

        return Math.Clamp(number, 0d, 100d);
    }

    protected override object ConvertBackCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var percent))
            return DependencyProperty.UnsetValue;

        percent = Math.Clamp(percent, 0d, 100d);
        if (targetType == typeof(int) || targetType == typeof(int?))
            return (int)Math.Round(percent, MidpointRounding.AwayFromZero);

        return percent / 100d;
    }
}
