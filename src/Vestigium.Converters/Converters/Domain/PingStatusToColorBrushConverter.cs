using System.Globalization;
using System.Windows.Media;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>CONV-17: ICMP / ping outcome → theme-aware frozen brush.</summary>
public sealed class PingStatusToColorBrushConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = Normalize(value);
        var (semantic, hex) = key switch
        {
            "success" or "ok" or "reply" => (BrushCache.Success, BrushCache.FallbackSuccess),
            "timeout" or "timedout" => (BrushCache.Warning, BrushCache.FallbackWarning),
            "destinationunreachable" or "unreachable" => (BrushCache.Danger, BrushCache.FallbackDanger),
            "ttlexpired" or "timeexceeded" or "ttl" => (BrushCache.AccentWarning, BrushCache.FallbackAccentWarning),
            _ => (BrushCache.Neutral, BrushCache.FallbackNeutral),
        };

        return ResolveBrush(semantic, hex);
    }

    private Brush ResolveBrush(string semantic, string hex)
        => GetService<IThemeBrushes>()?.TryGet(semantic) ?? BrushCache.FromHex(hex);

    private static string Normalize(object? value)
    {
        if (value is null)
            return string.Empty;

        var text = value.ToString() ?? string.Empty;
        return text.Replace("-", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace("_", "", StringComparison.Ordinal)
            .ToLowerInvariant();
    }
}
