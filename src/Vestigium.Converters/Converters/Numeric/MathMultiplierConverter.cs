using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Numeric;

/// <summary>CONV-13: value × ConverterParameter. ConvertBack divides when factor ≠ 0.</summary>
public sealed class MathMultiplierConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var number))
            return DependencyProperty.UnsetValue;
        if (!ConversionHelpers.TryParseParameterDouble(parameter, culture, out var factor))
            return DependencyProperty.UnsetValue;

        return Coerce(number * factor, targetType);
    }

    protected override object ConvertBackCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var number))
            return DependencyProperty.UnsetValue;
        if (!ConversionHelpers.TryParseParameterDouble(parameter, culture, out var factor) || factor == 0d)
            return Binding.DoNothing;

        return Coerce(number / factor, targetType);
    }

    private static object Coerce(double number, Type targetType)
    {
        if (targetType == typeof(int) || targetType == typeof(int?))
            return (int)Math.Round(number, MidpointRounding.AwayFromZero);
        if (targetType == typeof(float) || targetType == typeof(float?))
            return (float)number;
        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            return (decimal)number;
        return number;
    }
}
