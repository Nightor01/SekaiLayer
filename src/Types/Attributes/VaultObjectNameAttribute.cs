namespace SekaiLayer.Types.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class VaultObjectNameAttribute(string name) : Attribute
{
    public string Name => name;
}