using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Boolean;

/// <summary>CONV-06: not null → true (optional invert).</summary>
public sealed class NullToBooleanConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var present = value is not null;
        return ConversionHelpers.IsInvert(parameter) ? !present : present;
    }
}
