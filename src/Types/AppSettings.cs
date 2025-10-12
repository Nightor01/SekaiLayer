using System.Text.Json.Serialization;

namespace SekaiLayer.Types;

public class AppSettings
{
    [JsonPropertyName("start_minimized")]
    public bool StartMinimized { get; set; } = false;
    
    public AppSettings() {}

    public AppSettings(AppSettings appSettings)
    {
        StartMinimized = appSettings.StartMinimized;
    }
}