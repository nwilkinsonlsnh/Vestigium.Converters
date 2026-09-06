using System.Globalization;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Collections;

/// <summary>CONV-30: collection has items → true. Inverse supported.</summary>
public sealed class CollectionHasItemsToBooleanConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasItems = !CollectionEmptyToVisibilityConverter.IsEmpty(value);
        return ConversionHelpers.IsInvert(parameter) ? !hasItems : hasItems;
    }
}
