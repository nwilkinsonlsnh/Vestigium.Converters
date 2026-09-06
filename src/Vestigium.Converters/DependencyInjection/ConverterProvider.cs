using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Converters.Infrastructure;

namespace Vestigium.Converters.DependencyInjection;

public sealed class ConverterProvider : IConverterProvider
{
    private readonly IServiceProvider _services;
    private readonly ConcurrentDictionary<Type, object> _cache = new();

    public ConverterProvider(IServiceProvider services)
    {
        _services = services;
    }

    public T GetConverter<T>() where T : class
        => (T)GetConverter(typeof(T));

    public object GetConverter(Type converterType)
    {
        if (converterType is null)
            return FallbackConverter.Instance;

        return _cache.GetOrAdd(converterType, static (type, sp) => Resolve(type, sp), _services);
    }

    private static object Resolve(Type type, IServiceProvider sp)
    {
        try
        {
            var resolved = sp.GetService(type);
            if (resolved is not null)
                return resolved;

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
