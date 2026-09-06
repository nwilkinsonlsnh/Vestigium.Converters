using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>
/// CONV-40: string equality → Visibility. Default ignore-case.
/// <c>Ordinal|MX</c> is case-sensitive. <c>Inverse|MX</c> flips.
/// </summary>
public sealed class StringEqualsToVisibilityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null)
            return DependencyProperty.UnsetValue;

        ConversionHelpers.SplitFlags(parameter, out var expected, out var invert, out var ordinal);
        var left = value?.ToString() ?? string.Empty;
        var comparison = ordinal ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var equal = string.Equals(left, expected, comparison);
        var visible = invert ? !equal : equal;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }
}
