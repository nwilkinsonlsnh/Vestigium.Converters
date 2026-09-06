using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>
/// CONV-25: bool → semantic brush. Parameter <c>Success,Neutral</c> (true,false).
/// </summary>
public sealed class BooleanToBrushConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToBoolean(value, out var flag) && value is not null)
            return System.Windows.DependencyProperty.UnsetValue;

        var on = BrushCache.Success;
        var off = BrushCache.Neutral;
        ConversionHelpers.SplitFlags(parameter, out var expected, out _, out _);
        if (!string.IsNullOrWhiteSpace(expected))
        {
            var parts = expected.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1)
                on = parts[0];
            if (parts.Length >= 2)
                off = parts[1];
        }

        return ResolveSemanticBrush(flag ? on : off);
    }
}
