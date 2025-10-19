using System.Text.Json.Serialization;
using SekaiLayer.Types.Attributes;

namespace SekaiLayer.Types;

public class VaultObjectIdentifier
{
    public enum ObjectType
    {
        [VaultObjectName("Asset Group")]
        AssetGroup,
        [VaultObjectName("Image")]
        Image,
        [VaultObjectName("World")]
        World,
    }
    
    public required string Name { get; init; }
    
    public required Guid Id { get; init; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ObjectType Type { get; init; }

    public bool IsContentOpenable()
    {
        return Type
            is ObjectType.Image
            or ObjectType.World;
    }
}