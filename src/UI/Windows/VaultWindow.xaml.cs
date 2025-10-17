using SekaiLayer.Services;
using SekaiLayer.Types;

namespace SekaiLayer.UI.Windows;

public partial class VaultWindow
{
    private readonly VaultManager _vaultManager;
    public string VaultName => _vaultManager.VaultName;
    
    public VaultWindow(VaultManager vaultManager)
    {
        _vaultManager = vaultManager;
        
        InitializeComponent();
    }
}