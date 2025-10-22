using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Windows;
using SekaiLayer.Types.Data;

namespace SekaiLayer.Utils;

public static class GlobalOptions
{
    public static JsonSerializerOptions JsonSerializer() => new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            .WithAddedModifier(AddPolymorphicTypeInfo<AssetSettings>)
            .WithAddedModifier(AllowOnlyProperties<Rect, SelectRectProperties>)
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

    private static void AllowOnlyProperties<T0, T1>(JsonTypeInfo jsonTypeInfo) where T1 : ISelectProperties, new()
    {
        if (jsonTypeInfo.Type != typeof(T0))
        {
            return;
        }

        var selector = new T1();

        var copyList = jsonTypeInfo.Properties.ToList();
        
        foreach (var property in copyList)
        {
            if (!selector.Properties.Contains(property.Name))
            {
                jsonTypeInfo.Properties.Remove(property);
            }
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