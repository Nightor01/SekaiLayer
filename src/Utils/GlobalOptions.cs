using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Windows;
using JNS;
using SekaiLayer.Types.Data;

namespace SekaiLayer.Utils;

public static class GlobalOptions
{
    private static readonly JnsServer _server = new([typeof(AssetSettings), typeof(TileSetSettings)]);
    
    public enum Json
    {
        Default,
        RectangleSerialization,
        PolymorphicAssetSettings,
    }
    
    public static JsonSerializerOptions JsonSerializer(params Json[] selectedOptions)
    {
        var jsonOptions = new JsonSerializerOptions();

        foreach (var option in selectedOptions)
        {
            switch (option)
            {
                case Json.Default: AddDefaultJsonSerializerOptions(jsonOptions); break;
                case Json.RectangleSerialization: AddRectangleSerialization(jsonOptions); break;
                case Json.PolymorphicAssetSettings: AddPolymorphicAssetSettings(jsonOptions); break;
                
                default: Debug.Assert(false, "An error in `GlobalOptions.JsonSerializer` was encountered"); break;
            }
        }
        
        return jsonOptions;
    }

    private static void AddPolymorphicAssetSettings(JsonSerializerOptions options)
    {
        foreach (var converter in _server.Converters)
        {
            options.Converters.Add(converter);
        }
    }
    
    private static void AddRectangleSerialization(JsonSerializerOptions options)
    {
        options.TypeInfoResolver ??= new DefaultJsonTypeInfoResolver();

        options.TypeInfoResolver.WithAddedModifier(AllowOnlyProperties<Rect, SelectRectProperties>);
    }

    private static void AddDefaultJsonSerializerOptions(JsonSerializerOptions options)
    {
        options.WriteIndented = true;
    }

    private static void AllowOnlyProperties<T0, T1>(JsonTypeInfo jsonTypeInfo) where T1 : ISelectProperties, new()
    {
        if (jsonTypeInfo.Type != typeof(T0))
        {
            return;
        }

        var selector = new T1();

        var allowList = jsonTypeInfo.Properties
            .Where(x => selector.Properties.Contains(x.Name));
        jsonTypeInfo.Properties.Clear();

        foreach (var property in allowList)
        {
            jsonTypeInfo.Properties.Add(property);
        }
    }

    private interface ISelectProperties
    {
        public HashSet<string> Properties { get; } 
    }

    private class SelectRectProperties : ISelectProperties
    {
        public HashSet<string> Properties =>
        [
            "X", "Y", "Width", "Height"
        ];
    }
}