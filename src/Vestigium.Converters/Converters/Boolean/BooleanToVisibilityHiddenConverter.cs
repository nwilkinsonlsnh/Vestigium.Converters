using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-03: true → Visible, false/null → Hidden (layout preserved).</summary>
public sealed class BooleanToVisibilityHiddenConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToBoolean(value, out var flag) && value is not null)
            return DependencyProperty.UnsetValue;

        return flag ? Visibility.Visible : Visibility.Hidden;
    }

    protected override object ConvertBackCore(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility.Visible;
}
