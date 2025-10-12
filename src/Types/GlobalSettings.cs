using System.Text.Json.Serialization;

namespace SekaiLayer.Types;

public class GlobalSettings
{
    [JsonPropertyName("entries")]
    public required HashSet<VaultEntry> Entries { get; init; }
    [JsonPropertyName("appSettings")]
    public required AppSettings AppSettings { get; set; }
}