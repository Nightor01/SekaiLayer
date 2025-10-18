using System.IO;
using SekaiLayer.Types;
using SekaiLayer.Types.Exceptions;

namespace SekaiLayer.Services;

public class VaultManager
{
    private readonly VaultEntry _entry;
    public string VaultPath => _entry.Path;
    public string VaultName => _entry.Name;

    private const string _configDir = "Config";
    private static readonly string _configFile = Path.Combine(_configDir, "config.json");
    private const string _assetsDir = "Assets";
    private const string _worldsDir = "Worlds";
    
    public VaultManager(VaultEntry entry)
    {
        _entry = entry;

        if (!CheckVaultFiles(entry))
        {
            // TODO react
        }
    }
    
    /// <exception cref="VaultManagerException"></exception> 
    public static void PrepareVault(VaultEntry entry)
    {
        try
        {
            foreach (var dir in GetVaultDirectories(entry.Path))
            {
                Directory.CreateDirectory(dir);
            }

            foreach (var file in GetVaultFiles(entry.Path))
            {
                File.Create(file).Close();
            }
        }
        catch (Exception e) when (e
            is UnauthorizedAccessException
            or ArgumentException
            or PathTooLongException
            or DirectoryNotFoundException
            or IOException
            or NotSupportedException
        ) {
            throw new VaultManagerException(e.Message);
        }
    }

    /// <exception cref="VaultManagerException"></exception> 
    public static void RemoveVaultFiles(VaultEntry entry)
    {
        try
        {
            Directory.Delete(entry.Path, true);
        }
        catch (Exception e) when (e
             is UnauthorizedAccessException
             or ArgumentException
             or PathTooLongException
             or DirectoryNotFoundException
        ) {
            throw new VaultManagerException(e.Message);
        }
    }
    
    public static bool CheckVaultFiles(VaultEntry entry)
    {
        return GetVaultDirectories(entry.Path)
                   .Aggregate(true, (current, dir) => current && Directory.Exists(dir)) 
               && GetVaultFiles(entry.Path)
                   .Aggregate(true, (current, file) => current && File.Exists(file));
    }
    
    private static List<string> GetVaultDirectories(string path)
    {
        return
        [
            Path.Combine(path, _configDir),
            Path.Combine(path, _assetsDir),
            Path.Combine(path, _worldsDir),
        ];
    }

    private static List<string> GetVaultFiles(string path)
    {
        return
        [
            Path.Combine(path, _configFile)
        ];
    }
}