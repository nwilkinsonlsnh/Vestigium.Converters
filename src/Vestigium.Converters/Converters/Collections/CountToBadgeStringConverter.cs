using System.Collections;
using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Collections;

/// <summary>CONV-16: integer/count → badge string, capping at max (default 99+) .</summary>
public sealed class CountToBadgeStringConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!TryGetCount(value, culture, out var count))
            return DependencyProperty.UnsetValue;

        if (count < 0)
            count = 0;

        var max = 99;
        if (parameter is not null && ConversionHelpers.TryToInt(parameter, culture, out var parsed) && parsed > 0)
            max = parsed;

        return count > max
            ? string.Create(culture, $"{max}+")
            : count.ToString(culture);
    }

    private static bool TryGetCount(object? value, CultureInfo culture, out int count)
    {
        switch (value)
        {
            case ICollection collection:
                count = collection.Count;
                return true;
            case IEnumerable enumerable when enumerable.TryGetNonEnumeratedCount(out var n):
                count = n;
                return true;
            default:
                return ConversionHelpers.TryToInt(value, culture, out count);
        }
    }
}
