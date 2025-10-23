using JNS.Attributes;

namespace SekaiLayer.Types.Data;

[JsonPolyName("asset_type", nameof(AssetSettings), typeof(AssetSettings))]
public class AssetSettings
{
    public required VaultObjectIdentifier Id { get; init; }
    public required string FileName { get; init; } 
}