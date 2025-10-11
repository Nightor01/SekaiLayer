using System.Text.Json.Serialization;

namespace SekaiLayer.Types;

public class VaultEntry
{
    public enum EncryptionType
    {
        None,
    }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    [JsonPropertyName("path")]
    public required string Path { get; init; }
    [JsonPropertyName("encryption")]
    public required EncryptionType Encryption { get; init; }
}