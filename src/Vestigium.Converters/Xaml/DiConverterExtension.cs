using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Converters.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Vestigium.Converters.Xaml;

/// <summary>
/// Markup extension used as <c>{v:DiConverter ConverterType={x:Type v:BooleanToVisibilityConverter}}</c>.
/// </summary>
[MarkupExtensionReturnType(typeof(object))]
[ContentProperty(nameof(ConverterType))]
public class DiConverterExtension : MarkupExtension
{
    public DiConverterExtension()
    {
    }

    public DiConverterExtension(Type converterType)
    {
        ConverterType = converterType;
    }

    public Type? ConverterType { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var type = ConverterType;
        if (type is null)
            return FallbackConverter.Instance;

        if (IsDesignMode(serviceProvider) || VestigiumConverterHost.ServiceProvider is null)
            return CreateDesignTime(type);

        try
        {
            var hub = VestigiumConverterHost.ServiceProvider.GetService<IConverterProvider>();
            return hub?.GetConverter(type) ?? CreateDesignTime(type);
        }
        catch
        {
            return FallbackConverter.Instance;
        }
    }

    private static bool IsDesignMode(IServiceProvider serviceProvider)
    {
        try
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target
                && target.TargetObject is DependencyObject dobj)
            {
                return DesignerProperties.GetIsInDesignMode(dobj);
            }

            return DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }
        catch
        {
            return false;
        }
    }

    private static object CreateDesignTime(Type type)
    {
        try
        {
            if (type.IsAbstract || type.IsInterface)
                return FallbackConverter.Instance;

            return Activator.CreateInstance(type) ?? FallbackConverter.Instance;
        }
        catch
        {
            return FallbackConverter.Instance;
        }
    }
}

/// <summary>XAML surface <c>{v:Resolve}</c> required by the host integration contract.</summary>
[MarkupExtensionReturnType(typeof(object))]
[ContentProperty(nameof(ConverterType))]
public sealed class ResolveExtension : DiConverterExtension
{
    public ResolveExtension()
    {
    }

    public ResolveExtension(Type converterType)
        : base(converterType)
    {
    }
}
