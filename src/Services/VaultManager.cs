using System.IO;
using System.Security;
using System.Text.Json;
using SekaiLayer.Extensions;
using SekaiLayer.Types;
using SekaiLayer.Types.Exceptions;
using SekaiLayer.UI.Controls;

namespace SekaiLayer.Services;

public class VaultManager
{
    private readonly VaultEntry _entry;
    public string VaultPath => _entry.Path;
    public string VaultName => _entry.Name;
    
    private readonly VaultConfiguration _config;

    private static readonly JsonSerializerOptions _options = GlobalOptions.JsonSerializer();
    private const string _configDir = "Config";
    private static readonly string _configFile = Path.Combine(_configDir, "config.json");
    private const string _assetsDir = "Assets";
    private const string _worldsDir = "Worlds";
    
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
    public void AddWorld(string world)
    {
        if (_config.Worlds.Contains(x => x.Name == world))
        {
            throw new VaultManagerException("A world with this name already exists.");
        }
        
        AddWorldDirectory(world);
        
        _config.Worlds.Add(new()
        {
            Name = world,
            Id = Guid.CreateVersion7(),
            Type = VaultObjectIdentifier.ObjectType.World
        });   
        
        SaveConfiguration();
    }

    public IReadOnlyList<VaultObjectIdentifier> GetWorlds() => _config.Worlds;

    /// <exception cref="VaultManagerException"></exception>
    public void AddAssetGroup(string group)
    {
        if (_config.AssetGroups.Contains(x => x.Name == group))
        {
            throw new VaultManagerException("An asset group with this name already exists.");
        }
        
        AddAssetGroupDirectory(group);
        
        _config.AssetGroups.Add(new()
        {
            Name = group,
            Id = Guid.CreateVersion7(),
            Type =  VaultObjectIdentifier.ObjectType.AssetGroup
        });
        
        SaveConfiguration();
    }
    
    // TODO change placement of records
    /// <exception cref="VaultManagerException"></exception> 
    public void AddImage(VaultObjectIdentifier group, AddImageAssetControl.ReturnType data)
    {
        // TODO create generalized methode AddFileToGroup
        List<AssetSettings> config = GetGroupConfig(group);

        if (config.Contains(x => x.Id.Name == data.Name))
        {
            throw new VaultManagerException("An asset with this name already exists.");
        }

        var fileId = Guid.CreateVersion7();
        string fileName = fileId + Path.GetExtension(data.Path);
        string finalPath = GetGroupAssetFilePath(VaultPath, group.Name, fileName);
        
        CopyFile(data.Path, finalPath);
        
        var imageSettings = new AssetSettings()
        {
            Id = new VaultObjectIdentifier()
            {
                Name = data.Name,
                Type = VaultObjectIdentifier.ObjectType.Image,
                Id = fileId
            },
            FileName = fileName,
        };
        
        config.Add(imageSettings);
        
        GroupConfigUpdate(group, config);
    }

    /// <exception cref="VaultManagerException"></exception> 
    public List<VaultObjectIdentifier> GetAssetsFromGroup(VaultObjectIdentifier group)
    {
        return GetGroupConfig(group)
            .Select(x => x.Id)
            .ToList();
    }
    
    public IReadOnlyList<VaultObjectIdentifier> GetAssetGroups() => _config.AssetGroups;
    
    /// <exception cref="VaultManagerException"></exception> 
    public static void PrepareVault(VaultEntry entry)
    {
        try
        {
            foreach (var dir in GetVaultDirectories(entry.Path))
            {
                Directory.CreateDirectory(dir);
            }

            InitFiles(entry.Path);
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

    /// <exception cref="VaultManagerException"></exception> 
    private VaultConfiguration GetVaultConfiguration()
    {
        VaultConfiguration? configuration;
        
        try
        {
            string contents = File.ReadAllText(GetConfigFilePath(VaultPath));
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

    /// <exception cref="VaultManagerException"></exception> 
    private static void InitFiles(string path)
    {
        try
        {
            VaultConfiguration defaultConfiguration = new ()
            {
                CollaborationEnabled = false
            };
            string json = JsonSerializer.Serialize(defaultConfiguration, _options);
            File.WriteAllText(GetConfigFilePath(path), json);
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

    /// <exception cref="VaultManagerException"></exception> 
    private void AddWorldDirectory(string world)
    {
        string directoryPath = Path.Combine(GetWorldsDirPath(VaultPath), world);
        
        try
        {
            Directory.CreateDirectory(directoryPath);
            
            // TODO Add more logic
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
    private void AddAssetGroupDirectory(string assetGroup)
    {
        string directoryPath = Path.Combine(GetAssetsDirPath(VaultPath), assetGroup);
        
        try
        {
            Directory.CreateDirectory(directoryPath);
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
    private void SaveConfiguration()
    {
        try
        {
            string json = JsonSerializer.Serialize(_config, _options);
            File.WriteAllText(GetConfigFilePath(VaultPath), json);   
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
    
    /// <exception cref="VaultManagerException"></exception>
    private void GroupConfigUpdate(VaultObjectIdentifier group, List<AssetSettings> settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings, _options);
            File.WriteAllText(GetGroupAssetFilePath(VaultPath, group.Name, "config.json"), json);
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

    /// <exception cref="VaultManagerException"></exception>
    private List<AssetSettings> GetGroupConfig(VaultObjectIdentifier group)
    {
        string path = GetGroupAssetFilePath(VaultPath, group.Name, "config.json");

        if (!File.Exists(path))
            return [];
        
        List<AssetSettings>? groupConfig;
        
        try
        {
            string contents = File.ReadAllText(path);
            groupConfig = JsonSerializer.Deserialize<List<AssetSettings>>(contents);

            if (groupConfig is null)
                throw new VaultManagerException("Group configuration could not be deserialized from JSON");
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
        
        return groupConfig;
    }
    
    /// <exception cref="VaultManagerException"></exception>
    private void CopyFile(string oldPath, string newPath)
    {
        try
        {
            File.Copy(oldPath, newPath, false);
        }
        catch (Exception e) when (e
            is ArgumentException
            or UnauthorizedAccessException
            or IOException
            or NotSupportedException
        ) {
            throw new VaultManagerException(e.Message);  
        }
    }

    private static string GetGroupAssetFilePath(string path, string group, string fileName)
    {
        // TODO maybe add constant?
        return Path.Combine(path, _assetsDir, group, fileName);
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

    private static string GetConfigFilePath(string path)
    {
        return Path.Combine(path, _configFile);
    }

    private static string GetWorldsDirPath(string path)
    {
        return Path.Combine(path, _worldsDir);
    }
    
    private static string GetAssetsDirPath(string path)
    {
        return Path.Combine(path, _assetsDir);
    }
}