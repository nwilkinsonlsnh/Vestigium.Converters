using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-07: whitespace/empty/null → Collapsed, otherwise Visible.</summary>
public sealed class EmptyStringToVisibilityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string ?? value?.ToString();
        var visible = !string.IsNullOrWhiteSpace(text);
        if (ConversionHelpers.IsInvert(parameter))
            visible = !visible;

        return visible ? Visibility.Visible : Visibility.Collapsed;
    }
}
