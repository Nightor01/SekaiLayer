using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SekaiLayer.Types.Data;

namespace SekaiLayer.Utils;

public static class GlobalOptions
{
    public static JsonSerializerOptions JsonSerializer() => new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            .WithAddedModifier(AddPolymorphicTypeInfo<AssetSettings>)
    };

    private static void AddPolymorphicTypeInfo<T>(JsonTypeInfo jsonTypeInfo)
    {
        if (!typeof(T).IsAssignableFrom(jsonTypeInfo.Type) || jsonTypeInfo.Type.IsSealed)
        {
            return;
        }
        
        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions {
            TypeDiscriminatorPropertyName = "$subtype",
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };
            
        var types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(T)))
            .Select(t => new JsonDerivedType(t, t.Name.ToLowerInvariant()));

        foreach (var t in types)
        {
            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(t);
        }
    }
}