using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Formatting;

/// <summary>CONV-09: 1024-based B/KB/MB/GB/TB. 1048576 → "1.00 MB".</summary>
public sealed class BytesToReadableSizeConverter : BaseDiConverter
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB", "PB"];

    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var bytes) || bytes < 0)
            return DependencyProperty.UnsetValue;

        var decimals = 2;
        if (parameter is not null && ConversionHelpers.TryToInt(parameter, culture, out var parsed))
            decimals = Math.Clamp(parsed, 0, 6);

        var unit = 0;
        var size = bytes;
        while (size >= 1024d && unit < Units.Length - 1)
        {
            size /= 1024d;
            unit++;
        }

        var format = "F" + decimals.ToString(CultureInfo.InvariantCulture);
        return string.Concat(size.ToString(format, culture), " ", Units[unit]);
    }
}
