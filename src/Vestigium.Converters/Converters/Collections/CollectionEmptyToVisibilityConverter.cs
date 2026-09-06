using System.Collections;
using System.Globalization;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Collections;

/// <summary>CONV-15: null or Count == 0 → Collapsed (optional invert).</summary>
public sealed class CollectionEmptyToVisibilityConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var empty = IsEmpty(value);
        if (ConversionHelpers.IsInvert(parameter))
            empty = !empty;

        return empty ? Visibility.Collapsed : Visibility.Visible;
    }

    internal static bool IsEmpty(object? value)
    {
        switch (value)
        {
            case null:
                return true;
            case ICollection collection:
                return collection.Count == 0;
            case IEnumerable enumerable:
                var enumerator = enumerable.GetEnumerator();
                try
                {
                    return !enumerator.MoveNext();
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();
                }
            default:
                return false;
        }
    }
}
