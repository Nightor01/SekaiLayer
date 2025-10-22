using System.Windows;

namespace SekaiLayer.Types.Data;

public class TileSetSettings : AssetSettings
{
    public required int XCount { get; init; }
    public required int YCount { get; init; }
    public List<Rect> Exclusions { get; init; } = [];
}