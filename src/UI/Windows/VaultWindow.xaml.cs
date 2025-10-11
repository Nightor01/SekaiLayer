using SekaiLayer.Services;
using SekaiLayer.Types;

namespace SekaiLayer.UI.Windows;

public partial class VaultWindow
{
    public string VaultName { get; private set; }
    private readonly FileManager _fileManager;
    
    public VaultWindow(FileManager fileManager, string name)
    {
        _fileManager = fileManager;
        VaultName = name;
        
        InitializeComponent();
    }
}