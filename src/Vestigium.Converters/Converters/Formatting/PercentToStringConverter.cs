using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Formatting;

/// <summary>
/// CONV-28: number → percent string. Default treats 0–100.
/// <c>ConverterParameter=Fraction</c> treats 0–1 (0.042 → "4.2%").
/// Optional decimals: <c>Fraction|1</c> or <c>2</c>.
/// </summary>
public sealed class PercentToStringConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var number))
            return DependencyProperty.UnsetValue;

        var fraction = ConversionHelpers.HasFlag(parameter, "Fraction")
                       || ConversionHelpers.HasFlag(parameter, "Ratio");
        if (fraction)
            number *= 100d;

        var decimals = 1;
        ConversionHelpers.SplitFlags(parameter, out var expected, out _, out _);
        foreach (var part in expected.Split('|', ',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (ConversionHelpers.TryToInt(part, culture, out var parsed) && parsed is >= 0 and <= 6)
                decimals = parsed;
        }

        var format = "F" + decimals.ToString(CultureInfo.InvariantCulture);
        return string.Concat(number.ToString(format, culture), "%");
    }
}
