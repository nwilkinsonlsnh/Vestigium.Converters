using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>
/// CONV-35: TCP/UDP port probe state → theme-aware frozen brush.
/// Accepts enum or string; does not reference ProbeHost types.
/// </summary>
public sealed class PortStateToBrushConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = ConversionHelpers.NormalizeKey(value);
        var semantic = key switch
        {
            "open" or "listening" => BrushCache.Success,
            "openfiltered" => BrushCache.AccentWarning,
            "filtered" or "stealth" => BrushCache.Warning,
            "closed" or "reset" => BrushCache.Danger,
            _ => BrushCache.Neutral,
        };

        return ResolveSemanticBrush(semantic);
    }
}
