using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-22: value equals ConverterParameter → Visible.</summary>
public sealed class EqualityToVisibilityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null)
            return DependencyProperty.UnsetValue;

        ConversionHelpers.SplitFlags(parameter, out var expected, out var invert, out _);
        var equal = ConversionHelpers.AreEqual(value, expected, culture);
        var visible = invert ? !equal : equal;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }
}
