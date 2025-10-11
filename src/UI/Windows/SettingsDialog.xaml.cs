using SekaiLayer.Services;
using SekaiLayer.Types;

namespace SekaiLayer.UI.Windows;

public partial class SettingsDialog
{
    FileManager _fileManager;
    
    public SettingsDialog(FileManager fileManager)
    {
        _fileManager = fileManager;
        
        InitializeComponent();
    }
}