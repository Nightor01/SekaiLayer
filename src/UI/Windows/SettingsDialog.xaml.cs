using System.Windows;
using System.Windows.Controls;
using SekaiLayer.Services;
using SekaiLayer.Types;

namespace SekaiLayer.UI.Windows;

public partial class SettingsDialog
{
    private readonly FileManager _fileManager;
    
    public SettingsDialog(FileManager fileManager)
    {
        _fileManager = fileManager;
        
        InitializeComponent();
    }

    private void StartMinimizedCheckedStateChanged(object sender, RoutedEventArgs e)
    {
        CheckBox checkBox = (CheckBox)sender;

        if (checkBox.IsChecked is null)
            return;
        
        _fileManager.AppSettings.StartMinimized = checkBox.IsChecked.Value;
    }
}