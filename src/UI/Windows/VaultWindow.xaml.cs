using System.Windows;
using System.Windows.Controls;
using SekaiLayer.Services;

namespace SekaiLayer.UI.Windows;

public partial class VaultWindow
{
    private readonly VaultManager _vaultManager;
    public string VaultName => _vaultManager.VaultName;

    private enum TreeViewItems
    {
        Assets,
        Worlds
    }
    
    public VaultWindow(VaultManager vaultManager)
    {
        _vaultManager = vaultManager;
        
        InitializeComponent();
    }

    private void VaultWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var tvi in Enum.GetValues<TreeViewItems>())
        {
            TreeView.Items.Add(new TreeViewItem()
            {
                Header = tvi.ToString()
            });
        }

        var assets = ((TreeViewItem)TreeView.Items[(int)TreeViewItems.Assets]!);
        foreach (var gp in _vaultManager.GetAssetGroupNames())
        {
            assets.Items.Add(gp);
        }

        var worlds = ((TreeViewItem)TreeView.Items[(int)TreeViewItems.Worlds]!);
        foreach (var world in _vaultManager.GetWorldNames())
        {
            worlds.Items.Add(world);
        }
    }
}