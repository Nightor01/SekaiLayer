using System.IO;
using System.Security;
using System.Text.Json;
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
    private VaultConfiguration _config;
    
    /// <exception cref="VaultManagerException"></exception>
    public VaultManager(VaultEntry entry)
    {
        _entry = entry;

        if (!CheckVaultFiles(entry))
        {
            throw new VaultManagerException("Vault files do not have the correct structure.");
        }

        _config = GetVaultConfiguration();
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

            InitFiles();
        }
        catch (Exception e) when (e
            is UnauthorizedAccessException
            or ArgumentException
            or PathTooLongException
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
        
        // TODO recursive checking of individual worlds and assets
    }

    private VaultConfiguration GetVaultConfiguration()
    {
        VaultConfiguration? configuration;
        
        try
        {
            string contents = File.ReadAllText(_configFile);
            configuration = JsonSerializer.Deserialize<VaultConfiguration>(contents);

            if (configuration is null)
                throw new VaultManagerException("Vault could not be deserialized from JSON");
        }
        catch (Exception e) when (e
             is ArgumentException
             or PathTooLongException
             or DirectoryNotFoundException
             or IOException
             or UnauthorizedAccessException
             or NotSupportedException
             or SecurityException
             or JsonException
        ) {
            throw new VaultManagerException(e.Message);
        }

        return configuration;
    }

    private static void InitFiles()
    {
        try
        {
            VaultConfiguration defaultConfiguration = new ()
            {
                CollaborationEnabled = false
            };
            string json = JsonSerializer.Serialize(defaultConfiguration);
            File.WriteAllText(_configFile, json);
        }
        catch (Exception e) when (e
            is ArgumentException
            or PathTooLongException
            or DirectoryNotFoundException
            or IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or SecurityException
        ) {
            throw new VaultManagerException(e.Message);
        } 
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