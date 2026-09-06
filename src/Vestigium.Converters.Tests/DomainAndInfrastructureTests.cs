using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Converters.Converters.Domain;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Converters.Infrastructure;
using Xunit;

namespace Vestigium.Converters.Tests;

public sealed class DomainAndInfrastructureTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private sealed class BoomConverter : BaseDiConverter
    {
        protected override object ConvertCore(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new InvalidOperationException("boom");
    }

    private enum SamplePing
    {
        Success,
        Timeout,
        DestinationUnreachable,
        TtlExpired,
        Weird,
    }

    [Fact]
    public void Base_swallows_exceptions()
    {
        var c = new BoomConverter();
        Assert.Equal(DependencyProperty.UnsetValue, c.Convert(1, typeof(object), null, Culture));
    }

    [Fact]
    public void AddVestigiumConverters_registers_singletons()
    {
        var services = new ServiceCollection();
        services.AddVestigiumConverters();
        using var sp = services.BuildServiceProvider();

        var hub = sp.GetRequiredService<IConverterProvider>();
        var a = hub.GetConverter<PingStatusToColorBrushConverter>();
        var b = hub.GetConverter<PingStatusToColorBrushConverter>();
        Assert.Same(a, b);
        Assert.Same(a, sp.GetRequiredService<PingStatusToColorBrushConverter>());
        Assert.IsType<FallbackConverter>(hub.GetConverter(typeof(DomainAndInfrastructureTests)));
    }

    [Fact]
    public void Ping_status_maps_known_and_unknown()
    {
        var c = new PingStatusToColorBrushConverter();
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackSuccess), c.Convert(SamplePing.Success, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackWarning), c.Convert("Timeout", typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), c.Convert(SamplePing.DestinationUnreachable, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackAccentWarning), c.Convert(SamplePing.TtlExpired, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackNeutral), c.Convert(SamplePing.Weird, typeof(Brush), null, Culture));
    }

    [Fact]
    public void Latency_uses_defaults_and_parameter_override()
    {
        var c = new LatencyToSeverityColorConverter();
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackSuccess), c.Convert(20, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackWarning), c.Convert(50, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackWarning), c.Convert(150, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), c.Convert(151, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), c.Convert(80, typeof(Brush), "10,40", Culture));
        Assert.Equal(DependencyProperty.UnsetValue, c.Convert("x", typeof(Brush), null, Culture));
    }

    [Fact]
    public void Latency_reads_configuration_when_hosted()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Vestigium:Latency:GoodMilliseconds"] = "10",
                ["Vestigium:Latency:WarnMilliseconds"] = "20",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddVestigiumConverters();
        using var sp = services.BuildServiceProvider();
        VestigiumConverterHost.ServiceProvider = sp;
        try
        {
            var c = sp.GetRequiredService<LatencyToSeverityColorConverter>();
            Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), c.Convert(25, typeof(Brush), null, Culture));
        }
        finally
        {
            VestigiumConverterHost.ServiceProvider = null;
        }
    }

    [Fact]
    public void Http_phrases_and_unknown()
    {
        var c = new HttpStatusCodeToDescriptionConverter();
        Assert.Equal("200 OK", c.Convert(200, typeof(string), null, Culture));
        Assert.Equal("503 Service Unavailable", c.Convert(HttpStatusCode.ServiceUnavailable, typeof(string), null, Culture));
        Assert.Equal("599 Unknown", c.Convert(599, typeof(string), null, Culture));
        Assert.Equal(DependencyProperty.UnsetValue, c.Convert("abc", typeof(string), null, Culture));
    }

    [Fact]
    public void Dns_glyphs_and_unknown()
    {
        var c = new DnsRecordTypeToIconConverter();
        Assert.Equal(DnsRecordTypeToIconConverter.GlyphMx, c.Convert("mx", typeof(string), null, Culture));
        Assert.Equal(DnsRecordTypeToIconConverter.GlyphGeneric, c.Convert("NAPTR", typeof(string), null, Culture));
        Assert.Equal(string.Empty, c.Convert("A", typeof(string), "Path", Culture));
        Assert.Equal(string.Empty, c.Convert(null, typeof(string), null, Culture));
    }
}
