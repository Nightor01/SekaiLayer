using System.Windows;
using JNS.Attributes;

namespace SekaiLayer.Types.Data;

[JsonPolyName("asset_type", nameof(TileSetSettings), typeof(TileSetSettings))]
public class TileSetSettings : AssetSettings
{
    public required int XCount { get; init; }
    public required int YCount { get; init; }
    public List<Point> Exclusions { get; init; } = [];
    public bool AllowTurning { get; init; }
}