namespace SekaiLayer.Types.Data;

public class VaultConfiguration
{
    public required bool CollaborationEnabled { get; set; }
    public List<VaultObjectIdentifier> AssetGroups { get; init; } = [];
    public List<VaultObjectIdentifier> Worlds { get; init; } = [];
    public Dictionary<Guid, Guid> Dictionary { get; init; } = [];
}