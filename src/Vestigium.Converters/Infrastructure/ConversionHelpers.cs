using System.Globalization;

namespace Vestigium.Converters.Infrastructure;

internal static class ConversionHelpers
{
    public static CultureInfo CultureOrCurrent(CultureInfo? culture)
        => culture ?? CultureInfo.CurrentCulture;

    public static bool IsInvert(object? parameter)
    {
        if (parameter is bool flag)
            return flag;

        var text = parameter?.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.Equals("true", StringComparison.OrdinalIgnoreCase)
            || text.Equals("invert", StringComparison.OrdinalIgnoreCase)
            || text.Equals("inverse", StringComparison.OrdinalIgnoreCase)
            || text.Equals("remainder", StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryToBoolean(object? value, out bool result)
    {
        switch (value)
        {
            case bool b:
                result = b;
                return true;
            case bool? nb:
                result = nb == true;
                return true;
            case string s when bool.TryParse(s, out var parsed):
                result = parsed;
                return true;
            default:
                result = false;
                return false;
        }
    }

    public static bool TryToDouble(object? value, CultureInfo culture, out double result)
    {
        switch (value)
        {
            case double d:
                result = d;
                return !double.IsNaN(d) && !double.IsInfinity(d);
            case float f:
                result = f;
                return true;
            case decimal m:
                result = (double)m;
                return true;
            case byte or sbyte or short or ushort or int or uint or long or ulong:
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;
            case string s:
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                    return true;
                return double.TryParse(s, NumberStyles.Any, culture, out result);
            default:
                result = 0;
                return false;
        }
    }

    public static bool TryToInt(object? value, CultureInfo culture, out int result)
    {
        if (TryToDouble(value, culture, out var d) && d is >= int.MinValue and <= int.MaxValue)
        {
            result = (int)Math.Round(d, MidpointRounding.AwayFromZero);
            return true;
        }

        result = 0;
        return false;
    }

    public static bool TryParseParameterDouble(object? parameter, CultureInfo culture, out double result)
    {
        if (parameter is null)
        {
            result = 0;
            return false;
        }

        return TryToDouble(parameter, culture, out result);
    }
}
