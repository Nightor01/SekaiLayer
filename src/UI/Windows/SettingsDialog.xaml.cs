using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SekaiLayer.Services;
using SekaiLayer.Types;
using SekaiLayer.Types.Exceptions;

namespace SekaiLayer.UI.Windows;

public partial class SettingsDialog
{
    private readonly FileManager _fileManager;
    private readonly AppSettings _appSettings;
    
    public SettingsDialog(FileManager fileManager)
    {
        _fileManager = fileManager;
        _appSettings = new AppSettings(fileManager.AppSettings);
        
        InitializeComponent();
        
        Update.IsEnabled = false;
        StartMinimized.IsChecked = _fileManager.AppSettings.StartMinimized;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (Update.IsEnabled)
        {
            MessageBoxResult result = MessageBox.Show("You have unsaved changes, do you want to continue?", 
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No
                );
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
        }
        
        base.OnClosing(e);
    }

    private void EnableUpdateButton()
    {
        Update.IsEnabled = true;
    }

    private void StartMinimizedCheckedStateChanged(object sender, RoutedEventArgs e)
    {
        CheckBox checkBox = (CheckBox)sender;

        if (checkBox.IsChecked is null)
            return;
        
        _appSettings.StartMinimized = checkBox.IsChecked.Value;
        EnableUpdateButton();
    }

    private void UpdateSettings(object sender, RoutedEventArgs e)
    {
        try
        {
            _fileManager.UpdateAppSettings(_appSettings);
        }
        catch (FileManagerException ex)
        {
            MessageBox.Show("Settings could not be updated.\n" + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error 
                );
            return;
        }

        Update.IsEnabled = false;
    }
}