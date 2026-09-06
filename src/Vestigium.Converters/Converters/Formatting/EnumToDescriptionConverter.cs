using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Windows;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.Converters.Formatting;

/// <summary>CONV-08: [Description] then [Display(Name)], else member name.</summary>
public sealed class EnumToDescriptionConverter : BaseDiConverter
{
    protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return DependencyProperty.UnsetValue;

        var type = value.GetType();
        if (!type.IsEnum)
            return DependencyProperty.UnsetValue;

        var name = value.ToString() ?? string.Empty;
        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
        if (field is null)
            return name;

        var description = field.GetCustomAttribute<DescriptionAttribute>()?.Description;
        if (!string.IsNullOrWhiteSpace(description))
            return description;

        var display = field.GetCustomAttribute<DisplayAttribute>()?.GetName();
        return string.IsNullOrWhiteSpace(display) ? name : display;
    }
}
