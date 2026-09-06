using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>
/// CONV-42: value matches any of the comma-separated names in ConverterParameter.
/// Example: <c>A,AAAA,MX</c> or <c>Inverse|Timeout,TimedOut</c>.
/// </summary>
public sealed class EnumMatchToVisibilityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null || value is null)
            return Visibility.Collapsed;

        ConversionHelpers.SplitFlags(parameter, out var expected, out var invert, out _);
        if (string.IsNullOrWhiteSpace(expected))
            return DependencyProperty.UnsetValue;

        var key = ConversionHelpers.NormalizeKey(value);
        var match = false;
        foreach (var token in expected.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (key == ConversionHelpers.NormalizeKey(token))
            {
                match = true;
                break;
            }
        }

        var visible = invert ? !match : match;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }
}
