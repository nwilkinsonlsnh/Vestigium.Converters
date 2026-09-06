using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Numeric;

/// <summary>CONV-23: true when value &lt; ConverterParameter.</summary>
public sealed class IsLessThanToBooleanConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var number))
            return DependencyProperty.UnsetValue;
        if (!ConversionHelpers.TryParseParameterDouble(parameter, culture, out var threshold))
            return DependencyProperty.UnsetValue;

        return number < threshold;
    }
}
