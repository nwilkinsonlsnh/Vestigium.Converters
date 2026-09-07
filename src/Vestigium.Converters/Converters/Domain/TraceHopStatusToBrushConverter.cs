using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>
/// CONV-34: traceroute hop outcome → theme-aware frozen brush.
/// Accepts enum or string; does not reference TraceIQ types.
/// </summary>
public sealed class TraceHopStatusToBrushConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = ConversionHelpers.NormalizeKey(value);
        var semantic = key switch
        {
            "success" or "replied" or "reply" or "ok" => BrushCache.Success,
            "timeout" or "timedout" or "star" or "*" => BrushCache.Warning,
            "filtered" or "adminprohibited" or "prohibited" => BrushCache.AccentWarning,
            "unreachable" or "destinationunreachable" or "hostunreachable" or "netunreachable" => BrushCache.Danger,
            _ => BrushCache.Neutral,
        };

        return ResolveSemanticBrush(semantic);
    }
}
