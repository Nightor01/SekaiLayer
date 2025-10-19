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
}