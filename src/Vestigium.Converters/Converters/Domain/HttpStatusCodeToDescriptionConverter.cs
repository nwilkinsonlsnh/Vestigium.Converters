using System.Globalization;
using System.Net;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>CONV-19: HTTP code → "200 OK". Unknown codes → "{code} Unknown".</summary>
public sealed class HttpStatusCodeToDescriptionConverter : BaseDiConverter
{
    private static readonly Dictionary<int, string> Phrases = new()
    {
        [100] = "Continue",
        [101] = "Switching Protocols",
        [102] = "Processing",
        [200] = "OK",
        [201] = "Created",
        [202] = "Accepted",
        [204] = "No Content",
        [206] = "Partial Content",
        [301] = "Moved Permanently",
        [302] = "Found",
        [304] = "Not Modified",
        [307] = "Temporary Redirect",
        [308] = "Permanent Redirect",
        [400] = "Bad Request",
        [401] = "Unauthorized",
        [403] = "Forbidden",
        [404] = "Not Found",
        [405] = "Method Not Allowed",
        [408] = "Request Timeout",
        [409] = "Conflict",
        [410] = "Gone",
        [413] = "Payload Too Large",
        [415] = "Unsupported Media Type",
        [418] = "I'm a teapot",
        [422] = "Unprocessable Entity",
        [429] = "Too Many Requests",
        [500] = "Internal Server Error",
        [501] = "Not Implemented",
        [502] = "Bad Gateway",
        [503] = "Service Unavailable",
        [504] = "Gateway Timeout",
    };

    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int code;
        switch (value)
        {
            case HttpStatusCode status:
                code = (int)status;
                break;
            default:
                if (!ConversionHelpers.TryToInt(value, culture, out code))
                    return DependencyProperty.UnsetValue;
                break;
        }

        if (Phrases.TryGetValue(code, out var phrase))
            return $"{code} {phrase}";

        if (Enum.IsDefined(typeof(HttpStatusCode), code))
        {
            var name = ((HttpStatusCode)code).ToString();
            return string.Equals(name, code.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
                ? $"{code} Unknown"
                : $"{code} {SplitPascal(name)}";
        }

        return $"{code} Unknown";
    }

    private static string SplitPascal(string name)
    {
        Span<char> buffer = stackalloc char[name.Length * 2];
        var n = 0;
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i > 0 && char.IsUpper(c))
                buffer[n++] = ' ';
            buffer[n++] = c;
        }

        return new string(buffer[..n]);
    }
}
