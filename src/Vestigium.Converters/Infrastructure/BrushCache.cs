using System.Collections.Concurrent;
using System.Windows.Media;

namespace Vestigium.Converters.Infrastructure;

internal static class BrushCache
{
    public const string Success = "Success";
    public const string Warning = "Warning";
    public const string Danger = "Danger";
    public const string Neutral = "Neutral";
    public const string AccentWarning = "AccentWarning";

    public const string FallbackSuccess = "#22C55E";
    public const string FallbackWarning = "#EAB308";
    public const string FallbackDanger = "#EF4444";
    public const string FallbackNeutral = "#94A3B8";
    public const string FallbackAccentWarning = "#F97316";

    private static readonly ConcurrentDictionary<string, SolidColorBrush> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static SolidColorBrush FromHex(string hex)
        => Cache.GetOrAdd(hex, static key =>
        {
            var converted = ColorConverter.ConvertFromString(key) ?? Colors.Gray;
            var brush = new SolidColorBrush((Color)converted);
            if (brush.CanFreeze)
                brush.Freeze();
            return brush;
        });
}
