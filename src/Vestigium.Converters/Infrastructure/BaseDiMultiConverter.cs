using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vestigium.Converters.Infrastructure;

/// <summary>IMultiValueConverter sibling so the infrastructure claim is real (INF-24).</summary>
public abstract class BaseDiMultiConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return ConvertCore(values, targetType, parameter, ConversionHelpers.CultureOrCurrent(culture));
        }
        catch (Exception ex)
        {
            Log(ex);
            return DependencyProperty.UnsetValue;
        }
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        try
        {
            return ConvertBackCore(value, targetTypes, parameter, ConversionHelpers.CultureOrCurrent(culture));
        }
        catch (Exception ex)
        {
            Log(ex);
            return [DependencyProperty.UnsetValue];
        }
    }

    protected abstract object? ConvertCore(object[] values, Type targetType, object? parameter, CultureInfo culture);

    protected virtual object[]? ConvertBackCore(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => [Binding.DoNothing];

    private void Log(Exception ex)
    {
        try
        {
            var logger = VestigiumConverterHost.ServiceProvider?
                .GetService<ILoggerFactory>()?
                .CreateLogger(GetType());
            logger?.LogDebug(ex, "Multi converter {Converter} failed.", GetType().Name);
        }
        catch
        {
            // ignored
        }
    }
}
