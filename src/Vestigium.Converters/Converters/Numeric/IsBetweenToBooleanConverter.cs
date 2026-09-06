using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Numeric;

/// <summary>CONV-24: inclusive band. Parameter <c>50,150</c>. Optional |Inverse.</summary>
public sealed class IsBetweenToBooleanConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var number))
            return DependencyProperty.UnsetValue;
        if (!ConversionHelpers.TryParsePair(parameter, culture, out var lo, out var hi))
            return DependencyProperty.UnsetValue;

        if (lo > hi)
            (lo, hi) = (hi, lo);

        var inside = number >= lo && number <= hi;
        return ConversionHelpers.IsInvert(parameter) ? !inside : inside;
    }
}
