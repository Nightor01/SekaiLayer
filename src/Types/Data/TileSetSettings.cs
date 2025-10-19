using System.Drawing;

namespace SekaiLayer.Types.Data;

public class TileSetSettings : AssetSettings
{
    public required int XCount { get; init; }
    public required int YCount { get; init; }
    public List<Point> Empty { get; init; } = [];
}