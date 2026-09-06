using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>
/// CONV-20: DNS type → Segoe Fluent glyph (default) or empty path in v1 (OPEN-03).
/// </summary>
public sealed class DnsRecordTypeToIconConverter : BaseDiConverter
{
    public const string GlyphA = "\uE774";
    public const string GlyphAaaa = "\uE774";
    public const string GlyphMx = "\uE715";
    public const string GlyphTxt = "\uE8A5";
    public const string GlyphCname = "\uE71B";
    public const string GlyphNs = "\uE83F";
    public const string GlyphSoa = "\uE8F1";
    public const string GlyphPtr = "\uE72C";
    public const string GlyphSrv = "\uE90F";
    public const string GlyphCaa = "\uE72E";
    public const string GlyphGeneric = "\uE8A5";

    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return string.Empty;

        var mode = parameter?.ToString();
        if (mode is not null && mode.Equals("Path", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var key = (value.ToString() ?? string.Empty).Trim().ToUpperInvariant();
        return key switch
        {
            "A" => GlyphA,
            "AAAA" => GlyphAaaa,
            "MX" => GlyphMx,
            "TXT" => GlyphTxt,
            "CNAME" => GlyphCname,
            "NS" => GlyphNs,
            "SOA" => GlyphSoa,
            "PTR" => GlyphPtr,
            "SRV" => GlyphSrv,
            "CAA" => GlyphCaa,
            _ => GlyphGeneric,
        };
    }
}
