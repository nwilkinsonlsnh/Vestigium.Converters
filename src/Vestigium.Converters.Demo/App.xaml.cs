using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Converters.DependencyInjection;

namespace Vestigium.Converters.Demo;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        services.AddVestigiumConverters();
        VestigiumConverterHost.ServiceProvider = services.BuildServiceProvider();
        base.OnStartup(e);
    }
}
