using System.Collections.Concurrent;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.Extensions.Logging;

namespace Vestigium.Converters.Infrastructure;

/// <summary>
/// Sealed Convert/ConvertBack wrappers with fail-safe UnsetValue (INF-20…INF-25).
/// </summary>
public abstract class BaseDiConverter : IValueConverter
{
    private readonly ConcurrentDictionary<Type, object> _services = new();
    private ILogger? _logger;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (ReferenceEquals(value, DependencyProperty.UnsetValue) || ReferenceEquals(value, Binding.DoNothing))
            return DependencyProperty.UnsetValue;

        try
        {
            return ConvertCore(value, targetType, parameter, ConversionHelpers.CultureOrCurrent(culture));
        }
        catch (Exception ex)
        {
            Log(ex);
            return DependencyProperty.UnsetValue;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (ReferenceEquals(value, DependencyProperty.UnsetValue) || ReferenceEquals(value, Binding.DoNothing))
            return DependencyProperty.UnsetValue;

        try
        {
            return ConvertBackCore(value, targetType, parameter, ConversionHelpers.CultureOrCurrent(culture));
        }
        catch (Exception ex)
        {
            Log(ex);
            return DependencyProperty.UnsetValue;
        }
    }

    protected abstract object? ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture);

    protected virtual object? ConvertBackCore(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;

    /// <summary>
    /// Lazy optional service lookup. Misses are not cached so a host that assigns
    /// <see cref="VestigiumConverterHost.ServiceProvider"/> after first Convert still resolves.
    /// ConcurrentDictionary forbids null values — never store a miss.
    /// </summary>
    protected T? GetService<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var boxed) && boxed is T hit)
            return hit;

        var provider = VestigiumConverterHost.ServiceProvider;
        if (provider is null)
            return null;

        try
        {
            var resolved = provider.GetService(typeof(T));
            if (resolved is T typed)
            {
                _services[typeof(T)] = typed;
                return typed;
            }
        }
        catch
        {
            // Host resolution failures must not crash Convert (NFR-04).
        }

        return null;
    }

    private void Log(Exception ex)
    {
        try
        {
            _logger ??= GetService<ILoggerFactory>()?.CreateLogger(GetType());
            _logger?.LogDebug(ex, "Converter {Converter} failed.", GetType().Name);
        }
        catch
        {
            // A broken logger must never crash Convert (NFR-04).
        }
    }
}
