using System.Windows;
using SekaiLayer.Services;
using SekaiLayer.Types;

namespace SekaiLayer;

public partial class VaultWindow
{
    public string VaultName { get; private set; }
    private FileManager _fileManager;
    
    public VaultWindow(FileManager fileManager, VaultEntry entry)
    {
        _fileManager = fileManager;
        VaultName = entry.Name;
        
        InitializeComponent();
    }
}