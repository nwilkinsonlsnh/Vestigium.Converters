using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-05: not null → Visible, null → Collapsed (optional invert).</summary>
public sealed class NullToVisibilityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var visible = value is not null;
        if (ConversionHelpers.IsInvert(parameter))
            visible = !visible;

        return visible ? Visibility.Visible : Visibility.Collapsed;
    }
}
