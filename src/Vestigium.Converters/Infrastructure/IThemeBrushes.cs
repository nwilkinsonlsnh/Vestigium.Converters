using System.Windows.Media;

namespace Vestigium.Converters.Infrastructure;

/// <summary>
/// Optional theme bridge (OPEN-02). When registered, CONV-17 and CONV-18
/// request semantic brushes from here instead of frozen fallbacks.
/// </summary>
public interface IThemeBrushes
{
    Brush? TryGet(string semanticKey);
}
