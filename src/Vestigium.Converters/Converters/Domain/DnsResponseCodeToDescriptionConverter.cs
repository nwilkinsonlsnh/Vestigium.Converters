using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>
/// CONV-33: DNS RCODE / response name → short description.
/// Accepts enum, int, or string. Unknown values keep a readable fallback.
/// </summary>
public sealed class DnsResponseCodeToDescriptionConverter : BaseDiConverter
{
    private static readonly Dictionary<string, string> Phrases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["0"] = "No Error",
        ["noerror"] = "No Error",
        ["ok"] = "No Error",
        ["1"] = "Format Error",
        ["formerr"] = "Format Error",
        ["2"] = "Server Failure",
        ["servfail"] = "Server Failure",
        ["3"] = "Non-Existent Domain",
        ["nxdomain"] = "Non-Existent Domain",
        ["4"] = "Not Implemented",
        ["notimp"] = "Not Implemented",
        ["5"] = "Query Refused",
        ["refused"] = "Query Refused",
        ["6"] = "Name Exists when it should not",
        ["yxdomain"] = "Name Exists when it should not",
        ["9"] = "Not Authorized",
        ["notauth"] = "Not Authorized",
        ["timeout"] = "Timed Out",
        ["timedout"] = "Timed Out",
    };

    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return string.Empty;

        if (ConversionHelpers.TryToInt(value, culture, out var code)
            && Phrases.TryGetValue(code.ToString(CultureInfo.InvariantCulture), out var fromCode))
            return fromCode;

        var key = ConversionHelpers.NormalizeKey(value);
        if (Phrases.TryGetValue(key, out var phrase))
            return phrase;

        var raw = value.ToString() ?? string.Empty;
        return string.IsNullOrWhiteSpace(raw) ? "Unknown" : raw;
    }
}
