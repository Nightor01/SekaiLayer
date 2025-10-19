using System.Text.Json.Serialization;

namespace SekaiLayer.Types;

public class VaultObjectIdentifier
{
    public enum ObjectType
    {
        AssetGroup,
        Image,
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