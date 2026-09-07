using System.Globalization;
using System.Net;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Domain;

/// <summary>
/// CONV-27: HTTP status class → severity brush.
/// 1xx Neutral, 2xx Success, 3xx AccentWarning, 4xx Warning, 5xx Danger.
/// </summary>
public sealed class HttpStatusToSeverityBrushConverter : BaseDiConverter
{
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

        var semantic = code switch
        {
            >= 200 and <= 299 => BrushCache.Success,
            >= 300 and <= 399 => BrushCache.AccentWarning,
            >= 400 and <= 499 => BrushCache.Warning,
            >= 500 and <= 599 => BrushCache.Danger,
            _ => BrushCache.Neutral,
        };

        return ResolveSemanticBrush(semantic);
    }
}
