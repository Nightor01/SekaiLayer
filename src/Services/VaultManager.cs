using SekaiLayer.Types;
using SekaiLayer.Types.Exceptions;

namespace SekaiLayer.Services;

public class VaultManager(VaultEntry entry)
{
    public string VaultPath => entry.Path;
    public string VaultName => entry.Name;

    /// <exception cref="VaultManagerException"></exception> 
    public static void PrepareVault(VaultEntry entry)
    {
        // TODO Prepare vault
    }

    /// <exception cref="VaultManagerException"></exception> 
    public static void RemoveVaultFiles(VaultEntry entry)
    {
        // TODO Remove vault files
    }
}