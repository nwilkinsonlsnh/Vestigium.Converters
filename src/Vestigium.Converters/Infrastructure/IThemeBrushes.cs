using System.Windows.Media;

namespace Vestigium.Converters.Infrastructure;

/// <summary>
/// Optional theme bridge (OPEN-02). When registered, severity converters
/// request semantic brushes from here instead of frozen fallbacks.
/// </summary>
public interface IThemeBrushes
{
    Brush? TryGet(string semanticKey);
}
