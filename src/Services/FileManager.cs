using System.IO;
using System.Security;
using System.Text.Json;
using SekaiLayer.Types.Exceptions;
using SekaiLayer.Extensions;
using SekaiLayer.Types.Data;
using SekaiLayer.Utils;

namespace SekaiLayer.Services;

public class FileManager
{
    private static readonly JsonSerializerOptions _options = GlobalOptions.JsonSerializer();
    
    private readonly string _settingsPath;
    private readonly GlobalSettings _globalSettings;
    private HashSet<VaultEntry> _entries => _globalSettings.Entries;

    public IReadOnlyCollection<VaultEntry> Entries => _globalSettings.Entries;
    public AppSettings AppSettings => _globalSettings.AppSettings;
    
    /// <param name="settingsPath">Path to the FileManager settings file</param>
    /// <exception cref="FileManagerException"></exception>
    public FileManager(string settingsPath)
    {
        _settingsPath = settingsPath;
        _globalSettings = ReadSettings();
    }

    /// <exception cref="FileManagerException"></exception>
    public void UpdateAppSettings(AppSettings appSettings)
    {
        _globalSettings.AppSettings = appSettings;
        
        WriteSettings(_globalSettings);
    }
    
    /// <exception cref="FileManagerException"></exception>
    public VaultEntry GetEntry(string key)
    {
        var entry = _entries.FirstOrDefault(e => e.Name == key);

        if (entry is null)
            throw new FileManagerException($"Could not find an entry with name {key}");

        return entry;
    }

    /// <exception cref="FileManagerException"></exception>
    public void CreateVault(VaultEntry entry)
    {
        if (_entries.Contains(x => x.Name == entry.Name))
        {
            throw new FileManagerException($"Vault with the name {entry.Name} already exists!");
        }

        _entries.Add(entry);
        
        WriteSettings(_globalSettings);
    }

    /// <exception cref="FileManagerException"></exception>
    public void RemoveVault(string name)
    {
        var entry = _entries.FirstOrDefault(x => x.Name == name);

        if (entry is null)
            return;

        _entries.Remove(entry);
        
        WriteSettings(_globalSettings);
    }

    /// <exception cref="FileManagerException"></exception>
    private void WriteSettings(GlobalSettings settings)
    {
        try
        {
            string text = JsonSerializer.Serialize(settings, _options);
            File.WriteAllText(_settingsPath, text);
        }
        catch (Exception e) when (e
            is UnauthorizedAccessException
            or ArgumentException
            or IOException
            or NotSupportedException
            or SecurityException
        ) {
            throw new FileManagerException($"Failed to write settings file {_settingsPath}\n" + e.Message);
        }
    }
    
    /// <exception cref="FileManagerException"></exception>
    private GlobalSettings ReadSettings()
    {
        GlobalSettings? settings;
        
        if (!File.Exists(_settingsPath))
        {
            settings = new GlobalSettings()
            {
                Entries = [],
                AppSettings = new()
            };
            WriteSettings(settings);
            return settings;
        }

        try
        {
            string text = File.ReadAllText(_settingsPath);
            settings = JsonSerializer.Deserialize<GlobalSettings>(text);

            if (settings is null)
                throw new FileManagerException($"Entries read from {_settingsPath} ended up being `null`");
        }
        catch (Exception e) when (e
            is UnauthorizedAccessException
            or ArgumentException
            or IOException
            or NotSupportedException
            or SecurityException
            or JsonException
        ) {
            throw new FileManagerException($"Failed to read settings file {_settingsPath}\n" + e.Message);
        }

        return settings;
    }
}