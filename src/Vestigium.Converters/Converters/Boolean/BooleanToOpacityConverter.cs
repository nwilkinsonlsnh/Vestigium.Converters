using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-26: true/false → opacity pair. Default <c>1,0.35</c>.</summary>
public sealed class BooleanToOpacityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToBoolean(value, out var flag) && value is not null)
            return DependencyProperty.UnsetValue;

        var on = 1d;
        var off = 0.35d;
        if (ConversionHelpers.TryParsePair(parameter, culture, out var first, out var second))
        {
            on = first;
            off = second;
        }

        return flag ? on : off;
    }
}
