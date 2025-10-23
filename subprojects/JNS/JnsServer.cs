using System.Text.Json.Serialization;
using JNS.Attributes;
using JNS.Data;

namespace JNS;

public class JnsServer
{
    private readonly Dictionary<JnsRecord, Type> _forwardLookupZone = [];
    private readonly Dictionary<Type, JnsRecord> _backwardLookupZone = [];
    private readonly HashSet<string> _groups = [];
    private readonly List<JsonConverter> _converters = [];
    public IReadOnlyList<JsonConverter> Converters => _converters;
    
    public JnsServer(List<Type> includedTypes)
    {
        HashSet<Type> baseTypes = [];
        
        foreach (var includedType in includedTypes)
        {
            var attr = (JsonPolyNameAttribute)includedType.GetCustomAttributes(typeof(JsonPolyNameAttribute), true)
                .FirstOrDefault()!;
            
            _forwardLookupZone.Add(new(attr.GroupName, attr.TypeName), includedType);
            _backwardLookupZone.Add(includedType, new(attr.GroupName, attr.TypeName));
            _groups.Add(attr.GroupName);
            
            if (!baseTypes.Contains(attr.BaseType))
            {
                Type genericType = typeof(JnsConverter<>).MakeGenericType(attr.BaseType);
                var instance = (JsonConverter)Activator.CreateInstance(genericType, this)!;
                
                _converters.Add(instance);
            }
            
            baseTypes.Add(attr.BaseType);
        }
    }
    
    public bool ContainsGroup(string groupName) => _groups.Contains(groupName);

    public bool ContainsType(Type type) => _backwardLookupZone.ContainsKey(type);

    public Type? TranslateRecord(JnsRecord record)
    {
        _forwardLookupZone.TryGetValue(record, out var value);
        return value;
    }

    public JnsRecord? TranslateType<T>() => TranslateType(typeof(T));
    
    public JnsRecord? TranslateType(Type type)
    {
        _backwardLookupZone.TryGetValue(type, out var value);
        return value;
    }
}