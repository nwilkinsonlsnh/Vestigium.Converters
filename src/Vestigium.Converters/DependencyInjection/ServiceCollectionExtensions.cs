using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vestigium.Converters.Converters.Boolean;
using Vestigium.Converters.Converters.Collections;
using Vestigium.Converters.Converters.Domain;
using Vestigium.Converters.Converters.Formatting;
using Vestigium.Converters.Converters.Numeric;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IConverterProvider"/> and every v1 catalog converter as Singleton (INT-01, ARCH-20).
    /// </summary>
    public static IServiceCollection AddVestigiumConverters(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IConverterProvider, ConverterProvider>();
        services.TryAddSingleton<FallbackConverter>(_ => FallbackConverter.Instance);

        Register<BooleanToVisibilityConverter>(services);
        Register<InverseBooleanToVisibilityConverter>(services);
        Register<BooleanToVisibilityHiddenConverter>(services);
        Register<InverseBooleanConverter>(services);
        Register<NullToVisibilityConverter>(services);
        Register<NullToBooleanConverter>(services);
        Register<EmptyStringToVisibilityConverter>(services);
        Register<EnumToDescriptionConverter>(services);
        Register<BytesToReadableSizeConverter>(services);
        Register<TimeSpanToReadableConverter>(services);
        Register<StringToUpperConverter>(services);
        Register<IsGreaterThanToBooleanConverter>(services);
        Register<MathMultiplierConverter>(services);
        Register<PercentageToGridLengthConverter>(services);
        Register<CollectionEmptyToVisibilityConverter>(services);
        Register<CountToBadgeStringConverter>(services);
        Register<PingStatusToColorBrushConverter>(services);
        Register<LatencyToSeverityColorConverter>(services);
        Register<HttpStatusCodeToDescriptionConverter>(services);
        Register<DnsRecordTypeToIconConverter>(services);

        return services;
    }

    private static void Register<T>(IServiceCollection services) where T : class
        => services.TryAddSingleton<T>();
}
