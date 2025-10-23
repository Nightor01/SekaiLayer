namespace JNS.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class JsonPolyNameAttribute(string groupName, string typeName, Type baseType) : Attribute
{
    public string GroupName => groupName;
    public string TypeName => typeName;
    public Type BaseType => baseType;
}