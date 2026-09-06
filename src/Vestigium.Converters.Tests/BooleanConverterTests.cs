using System.Globalization;
using System.Windows;
using Vestigium.Converters.Converters.Boolean;
using Xunit;

namespace Vestigium.Converters.Tests;

public sealed class BooleanConverterTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    [Fact]
    public void BooleanToVisibility_maps_true_visible_false_collapsed()
    {
        var c = new BooleanToVisibilityConverter();
        Assert.Equal(Visibility.Visible, c.Convert(true, typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Collapsed, c.Convert(false, typeof(Visibility), null, Culture));
        Assert.Equal(true, c.ConvertBack(Visibility.Visible, typeof(bool), null, Culture));
        Assert.Equal(false, c.ConvertBack(Visibility.Collapsed, typeof(bool), null, Culture));
    }

    [Fact]
    public void InverseBooleanToVisibility_hides_when_true()
    {
        var c = new InverseBooleanToVisibilityConverter();
        Assert.Equal(Visibility.Collapsed, c.Convert(true, typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Visible, c.Convert(false, typeof(Visibility), null, Culture));
    }

    [Fact]
    public void Hidden_variant_preserves_layout()
    {
        var c = new BooleanToVisibilityHiddenConverter();
        Assert.Equal(Visibility.Hidden, c.Convert(false, typeof(Visibility), null, Culture));
    }

    [Fact]
    public void InverseBoolean_roundtrips()
    {
        var c = new InverseBooleanConverter();
        Assert.Equal(false, c.Convert(true, typeof(bool), null, Culture));
        Assert.Equal(true, c.ConvertBack(false, typeof(bool), null, Culture));
    }

    [Fact]
    public void NullToVisibility_and_boolean()
    {
        var vis = new NullToVisibilityConverter();
        var flag = new NullToBooleanConverter();
        Assert.Equal(Visibility.Collapsed, vis.Convert(null, typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Visible, vis.Convert("item", typeof(Visibility), null, Culture));
        Assert.Equal(false, flag.Convert(null, typeof(bool), null, Culture));
        Assert.Equal(false, flag.Convert(1, typeof(bool), "Inverse", Culture));
        Assert.Equal(true, flag.Convert(null, typeof(bool), "Inverse", Culture));
    }

    [Fact]
    public void EmptyString_collapses_whitespace()
    {
        var c = new EmptyStringToVisibilityConverter();
        Assert.Equal(Visibility.Collapsed, c.Convert("  ", typeof(Visibility), null, Culture));
        Assert.Equal(Visibility.Visible, c.Convert("ok", typeof(Visibility), null, Culture));
    }

    [Fact]
    public void Wrong_type_returns_unset()
    {
        var c = new BooleanToVisibilityConverter();
        Assert.Equal(DependencyProperty.UnsetValue, c.Convert("nope", typeof(Visibility), null, Culture));
    }
}
