using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Converters.Converters.Boolean;
using Vestigium.Converters.Converters.Collections;
using Vestigium.Converters.Converters.Domain;
using Vestigium.Converters.Converters.Formatting;
using Vestigium.Converters.Converters.Numeric;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Converters.Infrastructure;
using Xunit;

namespace Vestigium.Converters.Tests;

public sealed class CatalogV11Tests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private enum SampleHop
    {
        Replied,
        Timeout,
        Unreachable,
        Filtered,
    }

    private enum SamplePort
    {
        Open,
        Closed,
        Filtered,
        OpenFiltered,
    }

    [Fact]
    public void Catalog_v11_types_are_singletons()
    {
        var services = new ServiceCollection();
        services.AddVestigiumConverters();
        using var sp = services.BuildServiceProvider();

        Assert.Same(
            sp.GetRequiredService<TruncateStringConverter>(),
            sp.GetRequiredService<IConverterProvider>().GetConverter<TruncateStringConverter>());
        Assert.Same(
            sp.GetRequiredService<PacketLossToSeverityBrushConverter>(),
            sp.GetRequiredService<PacketLossToSeverityBrushConverter>());
        Assert.Same(
            sp.GetRequiredService<BooleanAndToVisibilityConverter>(),
            sp.GetRequiredService<BooleanAndToVisibilityConverter>());
    }

    [Fact]
    public void Equality_and_string_match()
    {
        var flag = new EqualityToBooleanConverter();
        var vis = new EqualityToVisibilityConverter();
        var text = new StringEqualsToVisibilityConverter();
        var match = new EnumMatchToVisibilityConverter();

        Assert.Equal(true, flag.Convert("MX", typeof(bool), "mx", Culture));
        Assert.Equal(false, flag.Convert("MX", typeof(bool), "Inverse|MX", Culture));
        Assert.Equal(Visibility.Visible, vis.Convert(200, typeof(Visibility), "200", Culture));
        Assert.Equal(Visibility.Collapsed, text.Convert("mx", typeof(Visibility), "Ordinal|MX", Culture));
        Assert.Equal(Visibility.Visible, text.Convert("MX", typeof(Visibility), "MX", Culture));
        Assert.Equal(Visibility.Visible, match.Convert("AAAA", typeof(Visibility), "A,AAAA,MX", Culture));
        Assert.Equal(Visibility.Collapsed, match.Convert("TXT", typeof(Visibility), "A,AAAA,MX", Culture));
        Assert.Equal(Visibility.Visible, match.Convert("TXT", typeof(Visibility), "Inverse|A,AAAA,MX", Culture));
    }

    [Fact]
    public void Invert_visibility_and_opacity()
    {
        var invert = new InvertVisibilityConverter();
        var opacity = new BooleanToOpacityConverter();

        Assert.Equal(Visibility.Collapsed, invert.Convert(Visibility.Visible, typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Visible, invert.Convert(Visibility.Hidden, typeof(Visibility), null, Culture));
        Assert.Equal(1d, opacity.Convert(true, typeof(double), null, Culture));
        Assert.Equal(0.2d, opacity.Convert(false, typeof(double), "1,0.2", Culture));
    }

    [Fact]
    public void Numeric_band_and_progress()
    {
        var less = new IsLessThanToBooleanConverter();
        var between = new IsBetweenToBooleanConverter();
        var progress = new ProgressToPercentConverter();

        Assert.Equal(true, less.Convert(3, typeof(bool), "10", Culture));
        Assert.Equal(false, less.Convert(10, typeof(bool), "10", Culture));
        Assert.Equal(true, between.Convert(50, typeof(bool), "50,150", Culture));
        Assert.Equal(false, between.Convert(200, typeof(bool), "50,150", Culture));
        Assert.Equal(true, between.Convert(200, typeof(bool), "Inverse|50,150", Culture));
        Assert.Equal(42d, progress.Convert(0.42, typeof(double), null, Culture));
        Assert.Equal(100d, progress.Convert(140, typeof(double), null, Culture));
        Assert.Equal(0.42d, progress.ConvertBack(42, typeof(double), null, Culture));
    }

    [Fact]
    public void Formatting_truncate_utc_percent_ms()
    {
        var truncate = new TruncateStringConverter();
        var utc = new UtcTimestampToStringConverter();
        var percent = new PercentToStringConverter();
        var ms = new MillisecondsToStringConverter();

        Assert.Equal("hello", truncate.Convert("hello", typeof(string), "24", Culture));
        Assert.Equal("hel…", truncate.Convert("hello", typeof(string), "4", Culture));
        Assert.Equal(string.Empty, truncate.Convert(null, typeof(string), null, Culture));

        var stamp = new DateTime(2026, 9, 6, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal("2026-09-06 12:00:00", utc.Convert(stamp, typeof(string), null, Culture));

        Assert.Equal("4.2%", percent.Convert(0.042, typeof(string), "Fraction|1", Culture));
        Assert.Equal("4.20%", percent.Convert(4.2, typeof(string), "2", Culture));
        Assert.Equal("42 ms", ms.Convert(42, typeof(string), null, Culture));
        Assert.Equal("1500.0 ms", ms.Convert(TimeSpan.FromSeconds(1.5), typeof(string), "1", Culture));
    }

    [Fact]
    public void Collection_has_items_and_multi_boolean()
    {
        var has = new CollectionHasItemsToBooleanConverter();
        var and = new BooleanAndToVisibilityConverter();
        var or = new BooleanOrToVisibilityConverter();

        Assert.Equal(false, has.Convert(Array.Empty<int>(), typeof(bool), null, Culture));
        Assert.Equal(true, has.Convert(new[] { 1 }, typeof(bool), null, Culture));
        Assert.Equal(true, has.Convert(Array.Empty<int>(), typeof(bool), "Inverse", Culture));

        Assert.Equal(Visibility.Visible, and.Convert([true, true], typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Collapsed, and.Convert([true, false], typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Visible, or.Convert([false, true], typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Collapsed, or.Convert([false, false], typeof(Visibility), null, Culture));
    }

    [Fact]
    public void Packet_loss_bands_and_fraction_parameter()
    {
        var c = new PacketLossToSeverityBrushConverter();
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackSuccess), c.Convert(0.2, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackWarning), c.Convert(1, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackWarning), c.Convert(5, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), c.Convert(6, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), c.Convert(0.08, typeof(Brush), "Fraction|1,5", Culture));
        Assert.Equal(DependencyProperty.UnsetValue, c.Convert("loss", typeof(Brush), null, Culture));
    }

    [Fact]
    public void Domain_status_brushes_and_dns_rcodes()
    {
        var http = new HttpStatusToSeverityBrushConverter();
        var hop = new TraceHopStatusToBrushConverter();
        var port = new PortStateToBrushConverter();
        var dns = new DnsResponseCodeToDescriptionConverter();
        var boolBrush = new BooleanToBrushConverter();

        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackSuccess), http.Convert(200, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackWarning), http.Convert(HttpStatusCode.NotFound, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), http.Convert(503, typeof(Brush), null, Culture));

        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackSuccess), hop.Convert(SampleHop.Replied, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackWarning), hop.Convert("*", typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), hop.Convert(SampleHop.Unreachable, typeof(Brush), null, Culture));

        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackSuccess), port.Convert(SamplePort.Open, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), port.Convert("closed", typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackAccentWarning), port.Convert(SamplePort.OpenFiltered, typeof(Brush), null, Culture));

        Assert.Equal("No Error", dns.Convert(0, typeof(string), null, Culture));
        Assert.Equal("Non-Existent Domain", dns.Convert("NXDOMAIN", typeof(string), null, Culture));
        Assert.Equal("Query Refused", dns.Convert(5, typeof(string), null, Culture));

        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackSuccess), boolBrush.Convert(true, typeof(Brush), null, Culture));
        Assert.Equal(BrushCache.FromHex(BrushCache.FallbackDanger), boolBrush.Convert(false, typeof(Brush), "Danger,Danger", Culture));
    }
}
