using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Vestigium.Converters.Converters.Collections;
using Vestigium.Converters.Converters.Formatting;
using Vestigium.Converters.Converters.Numeric;
using Xunit;

namespace Vestigium.Converters.Tests;

public sealed class FormattingAndNumericTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private enum Probe
    {
        [Description("Echo request")]
        Echo = 1,
        Bare = 2,
    }

    [Fact]
    public void Enum_uses_description_then_name()
    {
        var c = new EnumToDescriptionConverter();
        Assert.Equal("Echo request", c.Convert(Probe.Echo, typeof(string), null, Culture));
        Assert.Equal("Bare", c.Convert(Probe.Bare, typeof(string), null, Culture));
        Assert.Equal(DependencyProperty.UnsetValue, c.Convert(3, typeof(string), null, Culture));
    }

    [Fact]
    public void Bytes_formats_megabyte()
    {
        var c = new BytesToReadableSizeConverter();
        Assert.Equal("1.00 MB", c.Convert(1048576, typeof(string), null, Culture));
        Assert.Equal(DependencyProperty.UnsetValue, c.Convert(-1, typeof(string), null, Culture));
    }

    [Fact]
    public void TimeSpan_compact_omits_zero_units()
    {
        var c = new TimeSpanToReadableConverter();
        Assert.Equal("2h 15m 30s", c.Convert(new TimeSpan(2, 15, 30), typeof(string), null, Culture));
        Assert.Equal("0s", c.Convert(TimeSpan.Zero, typeof(string), null, Culture));
        Assert.Equal("2 hours 15 minutes 30 seconds", c.Convert(new TimeSpan(2, 15, 30), typeof(string), "Long", Culture));
    }

    [Fact]
    public void String_upper_null_is_empty()
    {
        var c = new StringToUpperConverter();
        Assert.Equal(string.Empty, c.Convert(null, typeof(string), null, Culture));
        Assert.Equal("PINGIQ", c.Convert("PingIQ", typeof(string), null, Culture));
    }

    [Fact]
    public void GreaterThan_requires_parameter()
    {
        var c = new IsGreaterThanToBooleanConverter();
        Assert.Equal(true, c.Convert(10, typeof(bool), "3", Culture));
        Assert.Equal(false, c.Convert(3, typeof(bool), "3", Culture));
        Assert.Equal(DependencyProperty.UnsetValue, c.Convert(10, typeof(bool), null, Culture));
    }

    [Fact]
    public void Multiplier_roundtrips()
    {
        var c = new MathMultiplierConverter();
        Assert.Equal(21d, c.Convert(7, typeof(double), "3", Culture));
        Assert.Equal(7d, c.ConvertBack(21, typeof(double), "3", Culture));
        Assert.Equal(Binding.DoNothing, c.ConvertBack(21, typeof(double), "0", Culture));
    }

    [Fact]
    public void Percentage_star_and_remainder()
    {
        var c = new PercentageToGridLengthConverter();
        var length = Assert.IsType<GridLength>(c.Convert(40, typeof(GridLength), null, Culture));
        Assert.Equal(40, length.Value);
        Assert.Equal(GridUnitType.Star, length.GridUnitType);

        var rem = Assert.IsType<GridLength>(c.Convert(40, typeof(GridLength), "Remainder", Culture));
        Assert.Equal(60, rem.Value);
    }

    [Fact]
    public void Collection_empty_and_badge()
    {
        var empty = new CollectionEmptyToVisibilityConverter();
        var badge = new CountToBadgeStringConverter();
        Assert.Equal(Visibility.Collapsed, empty.Convert(Array.Empty<int>(), typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Visible, empty.Convert(new[] { 1 }, typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Visible, empty.Convert(Array.Empty<int>(), typeof(Visibility), "Inverse", Culture));
        Assert.Equal("99+", badge.Convert(120, typeof(string), null, Culture));
        Assert.Equal("12", badge.Convert(12, typeof(string), null, Culture));
        Assert.Equal("2", badge.Convert(new[] { 1, 2 }, typeof(string), null, Culture));
        Assert.Equal("12", badge.Convert("12", typeof(string), null, Culture));
    }
}
