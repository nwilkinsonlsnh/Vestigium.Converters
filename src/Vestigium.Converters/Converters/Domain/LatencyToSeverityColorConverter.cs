using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>CONV-18: ms latency against Good/Warn thresholds → severity brush.</summary>
public sealed class LatencyToSeverityColorConverter : BaseDiConverter
{
    public const double DefaultGoodMilliseconds = 50d;
    public const double DefaultWarnMilliseconds = 150d;

    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double ms;
        if (value is TimeSpan span)
            ms = span.TotalMilliseconds;
        else if (!ConversionHelpers.TryToDouble(value, culture, out ms))
            return DependencyProperty.UnsetValue;

        GetThresholds(parameter, culture, out var good, out var warn);

        if (ms < good)
            return ResolveBrush(BrushCache.Success, BrushCache.FallbackSuccess);
        if (ms <= warn)
            return ResolveBrush(BrushCache.Warning, BrushCache.FallbackWarning);
        return ResolveBrush(BrushCache.Danger, BrushCache.FallbackDanger);
    }

    private Brush ResolveBrush(string semantic, string hex)
        => GetService<IThemeBrushes>()?.TryGet(semantic) ?? BrushCache.FromHex(hex);

    private void GetThresholds(object? parameter, CultureInfo culture, out double good, out double warn)
    {
        good = DefaultGoodMilliseconds;
        warn = DefaultWarnMilliseconds;

        var config = GetService<IConfiguration>();
        if (config is not null)
        {
            if (double.TryParse(config["Vestigium:Latency:GoodMilliseconds"], NumberStyles.Any, CultureInfo.InvariantCulture, out var cfgGood))
                good = cfgGood;
            if (double.TryParse(config["Vestigium:Latency:WarnMilliseconds"], NumberStyles.Any, CultureInfo.InvariantCulture, out var cfgWarn))
                warn = cfgWarn;
        }

        var text = parameter?.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return;

        var parts = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2
            && ConversionHelpers.TryToDouble(parts[0], culture, out var pGood)
            && ConversionHelpers.TryToDouble(parts[1], culture, out var pWarn))
        {
            good = pGood;
            warn = pWarn;
        }
    }
}
