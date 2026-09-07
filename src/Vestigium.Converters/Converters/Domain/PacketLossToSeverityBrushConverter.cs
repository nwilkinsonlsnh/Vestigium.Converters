using System.Globalization;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>
/// CONV-38: packet-loss percent → severity brush.
/// Default treats 0–100. <c>Fraction</c> treats 0–1 as a ratio.
/// Thresholds default to Good &lt; 1%, Warn ≤ 5%. Override with <c>1,5</c>
/// or <c>Vestigium:PacketLoss:GoodPercent</c> / <c>WarnPercent</c>.
/// </summary>
public sealed class PacketLossToSeverityBrushConverter : BaseDiConverter
{
    public const double DefaultGoodPercent = 1d;
    public const double DefaultWarnPercent = 5d;

    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConversionHelpers.TryToDouble(value, culture, out var number))
            return DependencyProperty.UnsetValue;

        var fraction = ConversionHelpers.HasFlag(parameter, "Fraction")
                       || ConversionHelpers.HasFlag(parameter, "Ratio");
        if (fraction && number <= 1d)
            number *= 100d;

        GetThresholds(parameter, culture, out var good, out var warn);

        if (number < good)
            return ResolveSemanticBrush(BrushCache.Success);
        if (number <= warn)
            return ResolveSemanticBrush(BrushCache.Warning);
        return ResolveSemanticBrush(BrushCache.Danger);
    }

    private void GetThresholds(object? parameter, CultureInfo culture, out double good, out double warn)
    {
        good = DefaultGoodPercent;
        warn = DefaultWarnPercent;

        var config = GetService<IConfiguration>();
        if (config is not null)
        {
            if (double.TryParse(config["Vestigium:PacketLoss:GoodPercent"], NumberStyles.Any, CultureInfo.InvariantCulture, out var cfgGood))
                good = cfgGood;
            if (double.TryParse(config["Vestigium:PacketLoss:WarnPercent"], NumberStyles.Any, CultureInfo.InvariantCulture, out var cfgWarn))
                warn = cfgWarn;
        }

        if (ConversionHelpers.TryParsePair(parameter, culture, out var first, out var second))
        {
            good = first;
            warn = second;
        }
    }
}
