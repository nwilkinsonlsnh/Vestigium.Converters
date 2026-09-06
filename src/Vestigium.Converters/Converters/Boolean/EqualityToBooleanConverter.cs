using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-21: value equals ConverterParameter → true. Prefix ! or |Inverse to flip.</summary>
public sealed class EqualityToBooleanConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null)
            return DependencyProperty.UnsetValue;

        ConversionHelpers.SplitFlags(parameter, out var expected, out var invert, out _);
        var equal = ConversionHelpers.AreEqual(value, expected, culture);
        return invert ? !equal : equal;
    }
}
