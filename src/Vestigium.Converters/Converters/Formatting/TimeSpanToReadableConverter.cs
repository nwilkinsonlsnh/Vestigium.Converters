using System.Globalization;
using System.Text;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Formatting;

/// <summary>CONV-10: compact "2h 15m 30s" (default) or long English units.</summary>
public sealed class TimeSpanToReadableConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        TimeSpan span;
        switch (value)
        {
            case TimeSpan ts:
                span = ts;
                break;
            case long ticks:
                span = TimeSpan.FromTicks(ticks);
                break;
            default:
                if (ConversionHelpers.TryToDouble(value, culture, out var ms))
                    span = TimeSpan.FromMilliseconds(ms);
                else
                    return DependencyProperty.UnsetValue;
                break;
        }

        if (span < TimeSpan.Zero)
            span = span.Duration();

        var longForm = parameter?.ToString()?.Equals("Long", StringComparison.OrdinalIgnoreCase) == true;
        return longForm ? FormatLong(span) : FormatCompact(span);
    }

    private static string FormatCompact(TimeSpan span)
    {
        if (span == TimeSpan.Zero)
            return "0s";

        var builder = new StringBuilder(24);
        Append(builder, span.Days, "d");
        Append(builder, span.Hours, "h");
        Append(builder, span.Minutes, "m");
        Append(builder, span.Seconds, "s");
        return builder.Length == 0 ? "0s" : builder.ToString();
    }

    private static string FormatLong(TimeSpan span)
    {
        if (span == TimeSpan.Zero)
            return "0 seconds";

        var builder = new StringBuilder(48);
        AppendWord(builder, span.Days, "day", "days");
        AppendWord(builder, span.Hours, "hour", "hours");
        AppendWord(builder, span.Minutes, "minute", "minutes");
        AppendWord(builder, span.Seconds, "second", "seconds");
        return builder.Length == 0 ? "0 seconds" : builder.ToString();
    }

    private static void Append(StringBuilder builder, int amount, string suffix)
    {
        if (amount <= 0)
            return;
        if (builder.Length > 0)
            builder.Append(' ');
        builder.Append(amount.ToString(CultureInfo.InvariantCulture));
        builder.Append(suffix);
    }

    private static void AppendWord(StringBuilder builder, int amount, string singular, string plural)
    {
        if (amount <= 0)
            return;
        if (builder.Length > 0)
            builder.Append(' ');
        builder.Append(amount.ToString(CultureInfo.InvariantCulture));
        builder.Append(' ');
        builder.Append(amount == 1 ? singular : plural);
    }
}
