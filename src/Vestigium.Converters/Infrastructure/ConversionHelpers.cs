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

        if (text.Equals("true", StringComparison.OrdinalIgnoreCase)
            || text.Equals("invert", StringComparison.OrdinalIgnoreCase)
            || text.Equals("inverse", StringComparison.OrdinalIgnoreCase)
            || text.Equals("remainder", StringComparison.OrdinalIgnoreCase))
            return true;

        SplitFlags(text, out _, out var invert, out _);
        return invert;
    }

    public static void SplitFlags(object? parameter, out string expected, out bool invert, out bool ordinal)
    {
        invert = false;
        ordinal = false;
        expected = parameter?.ToString()?.Trim() ?? string.Empty;
        if (expected.Length == 0)
            return;

        if (expected.StartsWith('!'))
        {
            invert = true;
            expected = expected[1..].Trim();
        }

        var parts = expected.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            expected = string.Empty;
            return;
        }

        var kept = new List<string>(parts.Length);
        foreach (var part in parts)
        {
            if (part.Equals("inverse", StringComparison.OrdinalIgnoreCase)
                || part.Equals("invert", StringComparison.OrdinalIgnoreCase))
            {
                invert = true;
                continue;
            }

            if (part.Equals("ordinal", StringComparison.OrdinalIgnoreCase))
            {
                ordinal = true;
                continue;
            }

            if (part.Equals("fraction", StringComparison.OrdinalIgnoreCase)
                || part.Equals("ratio", StringComparison.OrdinalIgnoreCase))
            {
                kept.Add(part);
                continue;
            }

            kept.Add(part);
        }

        expected = string.Join("|", kept);
    }

    public static bool HasFlag(object? parameter, string flag)
    {
        var text = parameter?.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (text.Equals(flag, StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var part in text.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.Equals(flag, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool TryToBoolean(object? value, out bool result)
    {
        switch (value)
        {
            case bool b:
                result = b;
                return true;
            case string s when bool.TryParse(s, out var parsed):
                result = parsed;
                return true;
            default:
                result = false;
                return false;
        }
    }

    public static bool CoerceBoolean(object? value)
    {
        if (TryToBoolean(value, out var flag))
            return flag;

        if (value is null)
            return false;

        if (TryToDouble(value, CultureInfo.InvariantCulture, out var number))
            return number != 0d;

        var text = value.ToString();
        return !string.IsNullOrWhiteSpace(text);
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

        SplitFlags(parameter, out var expected, out _, out _);
        return TryToDouble(string.IsNullOrWhiteSpace(expected) ? parameter : expected, culture, out result);
    }

    public static bool TryParsePair(object? parameter, CultureInfo culture, out double first, out double second)
    {
        first = 0;
        second = 0;
        SplitFlags(parameter, out var expected, out _, out _);
        if (string.IsNullOrWhiteSpace(expected))
            return false;

        var nums = new List<double>(2);
        foreach (var chunk in expected.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var token in chunk.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (TryToDouble(token, culture, out var number))
                    nums.Add(number);
            }
        }

        if (nums.Count < 2)
            return false;

        first = nums[0];
        second = nums[1];
        return true;
    }

    public static bool AreEqual(object? left, object? right, CultureInfo culture)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        if (left.Equals(right))
            return true;

        if (TryToDouble(left, culture, out var a) && TryToDouble(right, culture, out var b))
            return Math.Abs(a - b) < 1e-9;

        return string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeKey(object? value)
    {
        if (value is null)
            return string.Empty;

        var text = value.ToString() ?? string.Empty;
        return text.Replace("-", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace("_", "", StringComparison.Ordinal)
            .ToLowerInvariant();
    }

    public static string FallbackHex(string semantic)
        => semantic switch
        {
            BrushCache.Success => BrushCache.FallbackSuccess,
            BrushCache.Warning => BrushCache.FallbackWarning,
            BrushCache.Danger => BrushCache.FallbackDanger,
            BrushCache.AccentWarning => BrushCache.FallbackAccentWarning,
            _ => BrushCache.FallbackNeutral,
        };
}
