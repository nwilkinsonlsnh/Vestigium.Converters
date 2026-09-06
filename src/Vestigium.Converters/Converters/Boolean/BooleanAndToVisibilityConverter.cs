using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-31: MultiBinding AND of values → Visibility. Inverse supported.</summary>
public sealed class BooleanAndToVisibilityConverter : BaseDiMultiConverter
{
    protected override object ConvertCore(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
            return Visibility.Collapsed;

        foreach (var value in values)
        {
            if (ReferenceEquals(value, DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;
            if (!ConversionHelpers.CoerceBoolean(value))
                return ConversionHelpers.IsInvert(parameter) ? Visibility.Visible : Visibility.Collapsed;
        }

        return ConversionHelpers.IsInvert(parameter) ? Visibility.Collapsed : Visibility.Visible;
    }
}
