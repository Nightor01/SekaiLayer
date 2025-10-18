namespace SekaiLayer.Types;

public class VaultConfiguration
{
    public required bool CollaborationEnabled { get; set; }
    public List<VaultObjectIdentifier> AssetGroups { get; set; } = [];
    public List<VaultObjectIdentifier> Worlds { get; set; } = [];
}