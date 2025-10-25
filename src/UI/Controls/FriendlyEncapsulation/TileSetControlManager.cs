using System.Windows;
using System.Windows.Controls;
using SekaiLayer.Services;
using SekaiLayer.Types;
using SekaiLayer.Types.Data;
using SekaiLayer.Types.Exceptions;
using SkiaSharp;

namespace SekaiLayer.UI.Controls.FriendlyEncapsulation;

public class TileSetControlManager
{
    private readonly (TileSetSettings, string) _settings;
    public Control Control => _control;
    private readonly TileSetConfigurationControl _control;
    
    private readonly VaultManager _vaultManager;

    public TileSetControlManager(VaultObjectIdentifier tileSet, VaultManager manager)
    {
        _vaultManager = manager;
        _settings = GetSettings(tileSet, manager);
        
        _control = new(_settings.Item1.XCount, _settings.Item1.YCount, _settings.Item1.Exclusions)
        {
            Image = ReadImage(_settings.Item2)
        }; 
        
        // TODO remove
        _control.ApplyOk += ControlOnApplyOk;
        _control.ApplyCancel += ControlOnApplyCancel;
    }

    private void ControlOnApplyCancel(object? sender, EventArgs e)
    {
        _control.ExcludedTiles.Clear();
        _control.XCount = _settings.Item1.XCount;
        _control.YCount = _settings.Item1.YCount;

        foreach (var tile in _settings.Item1.Exclusions)
        {
            _control.ExcludedTiles.Add(tile);
        }
    }

    private void ControlOnApplyOk(object? sender, EventArgs e)
    {
        var settings = _settings.Item1;

        settings = new TileSetSettings()
        {
            XCount = _control.XCount,
            YCount = _control.YCount,
            Exclusions = _control.ExcludedTiles.ToList(),
            FileName = settings.FileName,
            Id = settings.Id,
        };

        try
        {
            _vaultManager.UpdateAsset(settings);
        }
        catch (VaultManagerException ex)
        {
            MessageBox.Show("Could not update settings.\n" + ex.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error
                );
            return;
        }
        
        MessageBox.Show("The tileset has been updated successfully", "Success",
            MessageBoxButton.OK, MessageBoxImage.Information
            );
    }
    
    private static (TileSetSettings, string) GetSettings(VaultObjectIdentifier tileSet, VaultManager manager)
    {
        try
        {
            var settings = manager.GetAssetSettings(tileSet);

            if (settings?.Item1 is not TileSetSettings)
            {
                throw new VaultManagerException("settings were not found");
            }

            return ((TileSetSettings)settings.Value.Item1, settings.Value.Item2);
        }
        catch (VaultManagerException e)
        {
            MessageBox.Show("Could not access this tileset's settins\n" + e.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error
                );
            return (null!, null!);
        }
    }

    private static SKImage? ReadImage(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            ShowImageError();
            return null;
        }

        var image = SKImage.FromEncodedData(path);

        if (image is null)
        {
            ShowImageError();
        }

        return image;
    }

    private static void ShowImageError()
    {
        MessageBox.Show("Could not read this tileset", "Error",
            MessageBoxButton.OK, MessageBoxImage.Error
            );
    }
}