using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Formatting;

/// <summary>CONV-36: truncate with ellipsis. Parameter is max length (default 24).</summary>
public sealed class TruncateStringConverter : BaseDiConverter
{
    public const int DefaultLength = 24;

    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return string.Empty;

        var text = value as string ?? value.ToString() ?? string.Empty;
        var max = DefaultLength;
        if (parameter is not null && ConversionHelpers.TryToInt(parameter, culture, out var parsed) && parsed > 0)
            max = parsed;

        if (text.Length <= max)
            return text;

        if (max <= 1)
            return "…";

        return string.Concat(text.AsSpan(0, max - 1), "…");
    }
}
