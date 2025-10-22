using System.Windows;

namespace SekaiLayer.Types;

public static class ImportTypes
{
    public record AssetGroup(string Name);
    
    /// <param name="Name">Display name for the image</param>
    /// <param name="Path">The path from which the image should be imported</param>
    public record Image(
        string Name,
        string Path
    );

    /// <param name="Name">Display name for the image</param>
    /// <param name="Path">The path from which the image should be imported</param>
    /// <param name="XCount">Number of tiles on the X direction</param>
    /// <param name="YCount">Number of tiles on the Y direction</param>
    /// <param name="Exclusions">Points that should not be included in the tileset</param>
    public record TileSet(
        string Name,
        string Path,
        int XCount,
        int YCount,
        List<Rect> Exclusions
    );
}