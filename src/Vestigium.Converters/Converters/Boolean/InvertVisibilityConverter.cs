using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-41: Visible ↔ Collapsed. Hidden maps to Visible.</summary>
public sealed class InvertVisibilityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
            return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        if (ConversionHelpers.TryToBoolean(value, out var flag))
            return flag ? Visibility.Collapsed : Visibility.Visible;

        return DependencyProperty.UnsetValue;
    }

    protected override object ConvertBackCore(object? value, Type targetType, object? parameter, CultureInfo culture)
        => ConvertCore(value, targetType, parameter, culture);
}
