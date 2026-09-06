using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-32: MultiBinding OR of values → Visibility. Inverse supported.</summary>
public sealed class BooleanOrToVisibilityConverter : BaseDiMultiConverter
{
    protected override object ConvertCore(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
            return Visibility.Collapsed;

        var any = false;
        foreach (var value in values)
        {
            if (ReferenceEquals(value, DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;
            if (ConversionHelpers.CoerceBoolean(value))
                any = true;
        }

        var visible = ConversionHelpers.IsInvert(parameter) ? !any : any;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }
}
