using System.Text.Json.Serialization;

namespace SekaiLayer.Types;

public class VaultEntry
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    [JsonPropertyName("path")]
    public required string Path { get; init; }
}