namespace SekaiLayer.Types;

public class VaultConfiguration
{
    public required bool CollaborationEnabled { get; set; }
    public List<string> AssetGroups { get; set; } = [];
    public List<string> Worlds { get; set; } = [];
}