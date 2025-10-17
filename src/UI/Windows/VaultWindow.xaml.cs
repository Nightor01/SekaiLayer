using SekaiLayer.Types;

namespace SekaiLayer.UI.Windows;

public partial class VaultWindow
{
    private readonly VaultEntry _entry;
    public string VaultPath => _entry.Path;
    public string VaultName => _entry.Name;
    
    public VaultWindow(VaultEntry entry)
    {
        _entry = entry;
        
        InitializeComponent();
    }
}