using System.Text.Json;

namespace SekaiLayer.Services;

public static class GlobalOptions
{
    public static JsonSerializerOptions JsonSerializer() => new()
    {
        WriteIndented = true,
    };
}