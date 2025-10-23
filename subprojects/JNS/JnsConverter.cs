using System.Text.Json;
using System.Text.Json.Serialization;
using JNS.Data;

namespace JNS;

public class JnsConverter<T>(JnsServer server) : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Could not find the start of an object");
        }
        
        if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException("Could not find a property name");
        }

        string? typeDescriminator = reader.GetString();

        if (typeDescriminator is null || !server.ContainsGroup(typeDescriminator))
        {
            throw new JsonException("This type-group is not recognized by this JNS server");
        }
        
        if (!reader.Read() || reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        string? name = reader.GetString();

        if (name is null)
        {
            throw new JsonException("Name for type is not present");
        }
        
        Type? type = server.TranslateRecord(new JnsRecord( typeDescriminator, name));

        if (type is null)
        {
            throw new JsonException("Type could not be found on the JNS server");
        }

        if (!reader.Read() || reader.GetString() != "type_value")
        {
            throw new JsonException("No field type_value found");
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Object definition could not be found");
        }

        var baseClass = (T)JsonSerializer.Deserialize(ref reader, type)!;

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException();
        }

        return baseClass;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value is null)
        {
            JnsRecord baseRecord = server.TranslateType<T>()!;
            writer.WriteString(baseRecord.GroupName, baseRecord.TypeName);
            writer.WritePropertyName("type_value");

            JsonSerializer.Serialize(writer, null, typeof(T));    
            
            writer.WriteEndObject();

            return;
        }

        JnsRecord? record = server.TranslateType(value.GetType());

        if (record is null)
        {
            throw new NotSupportedException("Type could not be found");
        }

        writer.WriteString(record.GroupName, record.TypeName);
        writer.WritePropertyName("type_value");
        
        JsonSerializer.Serialize(writer, value, value.GetType());

        writer.WriteEndObject();
    }
}