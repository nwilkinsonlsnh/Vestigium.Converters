namespace Vestigium.Converters.DependencyInjection;

/// <summary>
/// Single factory/locator for catalog converters (INF-01).
/// </summary>
public interface IConverterProvider
{
    object GetConverter(Type converterType);

    T GetConverter<T>() where T : class;
}
