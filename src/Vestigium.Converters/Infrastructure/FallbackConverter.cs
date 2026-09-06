using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Vestigium.Converters.Infrastructure;

/// <summary>Safe no-op used when a type cannot be resolved (INF-03, INT-05).</summary>
public sealed class FallbackConverter : IValueConverter, IMultiValueConverter
{
    public static FallbackConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => [DependencyProperty.UnsetValue];
}
