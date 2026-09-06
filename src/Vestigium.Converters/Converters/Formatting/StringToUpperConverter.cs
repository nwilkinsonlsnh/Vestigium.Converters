using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Formatting;

/// <summary>CONV-11: culture-aware uppercase. null → empty string.</summary>
public sealed class StringToUpperConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return string.Empty;

        var text = value as string ?? value.ToString() ?? string.Empty;
        return text.ToUpper(culture);
    }
}
