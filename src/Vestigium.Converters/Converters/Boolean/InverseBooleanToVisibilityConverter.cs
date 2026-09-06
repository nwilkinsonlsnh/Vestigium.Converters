using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-02: true → Collapsed, false/null → Visible.</summary>
public sealed class InverseBooleanToVisibilityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToBoolean(value, out var flag) && value is not null)
            return DependencyProperty.UnsetValue;

        return flag ? Visibility.Collapsed : Visibility.Visible;
    }

    protected override object ConvertBackCore(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility.Collapsed or Visibility.Hidden;
}
