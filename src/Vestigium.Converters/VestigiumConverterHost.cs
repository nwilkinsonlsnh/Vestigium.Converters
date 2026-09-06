namespace Vestigium.Converters;

/// <summary>
/// One-line host hook. Assign the built <see cref="IServiceProvider"/> at WPF startup
/// so <c>{v:Resolve}</c> can locate singleton converters.
/// </summary>
public static class VestigiumConverterHost
{
    public static IServiceProvider? ServiceProvider { get; set; }
}
