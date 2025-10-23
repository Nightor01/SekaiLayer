using System.IO;
using System.Security;
using System.Text.Json;
using SekaiLayer.Extensions;
using SekaiLayer.Types;
using SekaiLayer.Types.Data;
using SekaiLayer.Types.Exceptions;
using SekaiLayer.Utils;

namespace SekaiLayer.Services;

public class VaultManager
{
    
    private readonly VaultEntry _entry;
    private readonly VaultConfiguration _config;

    private static readonly JsonSerializerOptions _options;
    private const string _generalConfigFile = "config.json";
    private const string _configDir = "Config";
    private static readonly string _configFile;
    private const string _assetsDir = "Assets";
    private const string _worldsDir = "Worlds";

    public string VaultPath => _entry.Path;
    public string VaultName => _entry.Name;
    public IReadOnlyDictionary<Guid, Guid> Dictionary => _config.Dictionary;

    static VaultManager()
    {
        _configFile = Path.Combine(_configDir, _generalConfigFile);
        _options = GlobalOptions.JsonSerializer(
            GlobalOptions.Json.Default,
            GlobalOptions.Json.RectangleSerialization,
            GlobalOptions.Json.PolymorphicAssetSettings
        );
    }
    
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
    public (AssetSettings, string)? GetAssetSettings(VaultObjectIdentifier asset)
    {
        if (!Dictionary.TryGetValue(asset.Id, out var groupId))
        {
            throw new VaultManagerException("Asset group not found.");
        }

        VaultObjectIdentifier group = GetGroupIdentifier(groupId);
        
        var config = GetGroupConfig(group);

        AssetSettings? settings = config.FirstOrDefault(x => x.Id.Id == asset.Id);

        if (settings is null)
        {
            return null;
        }
        
        return (settings, GetGroupAssetFilePath(VaultPath, group.Name, settings.FileName));
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

    /// <exception cref="VaultManagerException"></exception> 
    public void UpdateAsset(AssetSettings settings)
    {
        if (!Dictionary.TryGetValue(settings.Id.Id, out var groupId))
        {
            throw new VaultManagerException("Asset group not found.");
        }
        
        VaultObjectIdentifier group = GetGroupIdentifier(groupId);
        
        var list = GetGroupConfig(group);
        
        var (index, asset) = list
            .Index()
            .FirstOrDefault(x => x.Item.Id.Id == settings.Id.Id
                                 && x.Item.FileName == settings.FileName
                                 && x.Item.Id.Type ==  settings.Id.Type
                                 );

        // TODO: Does it work like this here?
        if (asset is null)
        {
            throw new VaultManagerException("Asset could not be found");
        }

        list[index] = settings;
        
        GroupConfigUpdate(group, list);
    }
    
    /// <exception cref="VaultManagerException"></exception> 
    public void AddImage(VaultObjectIdentifier group, ImportTypes.Image data)
    {
        AddFileToGroup(
            group,
            data.Path,
            data.Name,
            VaultObjectIdentifier.ObjectType.Image,
            DefaultSettingsCreator
            );
    }

    /// <exception cref="VaultManagerException"></exception> 
    public void AddTileSet(VaultObjectIdentifier group, ImportTypes.TileSet data)
    {
        AddFileToGroup(group, data.Path, data.Name, VaultObjectIdentifier.ObjectType.TileSet, Func);
        
        AssetSettings Func(VaultObjectIdentifier id, string fileName)
        {
            return new TileSetSettings()
            {
                Id = id,
                FileName = fileName,
                XCount = data.XCount,
                YCount = data.YCount,
                Exclusions = data.Exclusions
            };
        }
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
            configuration = JsonSerializer.Deserialize<VaultConfiguration>(contents, _options);

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
            string configJson = JsonSerializer.Serialize(defaultConfiguration, _options);
            File.WriteAllText(GetConfigFilePath(path), configJson);
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
            File.WriteAllText(GetGroupAssetFilePath(VaultPath, group.Name, _generalConfigFile), json);
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
        string path = GetGroupAssetFilePath(VaultPath, group.Name, _generalConfigFile);

        if (!File.Exists(path))
            return [];
        
        List<AssetSettings>? groupConfig;
        
        try
        {
            string contents = File.ReadAllText(path);
            groupConfig = JsonSerializer.Deserialize<List<AssetSettings>>(contents, _options);

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
    
    
    /// <summary>
    /// Adds a file to group using provided arguments
    /// </summary>
    /// <param name="group">Group where the file should be placed</param>
    /// <param name="oldPath">Path of the file to be imported</param>
    /// <param name="name">Name of the asset</param>
    /// <param name="type">Type of the asset</param>
    /// <param name="settingsCreator">A functor that takes in an id created by this function and a name created by this
    /// function and creates an AssetSettings object accordingly</param>
    /// <exception cref="VaultManagerException"></exception>
    private void AddFileToGroup(VaultObjectIdentifier group, string oldPath, string name,
        VaultObjectIdentifier.ObjectType type, Func<VaultObjectIdentifier, string, AssetSettings> settingsCreator
    ) {
        List<AssetSettings> config = GetGroupConfig(group);

        if (config.Contains(x => x.Id.Name == name))
        {
            throw new VaultManagerException("An asset with this name already exists.");
        }

        var fileId = Guid.CreateVersion7();
        string fileName = fileId + Path.GetExtension(oldPath);
        string finalPath = GetGroupAssetFilePath(VaultPath, group.Name, fileName);
        
        CopyFile(oldPath, finalPath);

        var id = new VaultObjectIdentifier()
        {
            Name = name,
            Type = type,
            Id = fileId
        };
        var imageSettings = settingsCreator(id, fileName);
        
        config.Add(imageSettings);
        
        GroupConfigUpdate(group, config);
        
        _config.Dictionary.Add(fileId, group.Id);
        SaveConfiguration();
    }

    private static AssetSettings DefaultSettingsCreator(VaultObjectIdentifier id, string fileName)
    {
        return new AssetSettings()
        {
            Id = id,
            FileName = fileName
        };
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

    /// <exception cref="VaultManagerException"></exception>
    private VaultObjectIdentifier GetGroupIdentifier(Guid guid)
    {
        VaultObjectIdentifier? group = GetAssetGroups().FirstOrDefault(x => x.Id == guid);

        if (group is null)
        {
            throw new VaultManagerException("Asset group not found.");
        }

        return group;
    }
    
    private static string GetGroupAssetFilePath(string path, string group, string fileName)
    {
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