using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SekaiLayer.Services;
using SekaiLayer.Types;
using SekaiLayer.UI.Controls;

namespace SekaiLayer.UI.Windows;

public partial class VaultWindow
{
    private readonly VaultManager _vaultManager;
    public string VaultName => _vaultManager.VaultName;
    private readonly ObservableCollection<HeaderOnlyTabItem> _tabControlData = [];

    private enum TreeViewItems
    {
        Assets,
        Worlds
    }
    
    public VaultWindow(VaultManager vaultManager)
    {
        _vaultManager = vaultManager;
        
        InitializeComponent();
        
        TabControl.ItemsSource = _tabControlData;
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
        foreach (var gp in _vaultManager.GetAssetGroups())
        {
            assets.Items.Add(MakeTreeViewItem(gp));
        }

        var worlds = ((TreeViewItem)TreeView.Items[(int)TreeViewItems.Worlds]!);
        foreach (var world in _vaultManager.GetWorlds())
        {
            worlds.Items.Add(MakeTreeViewItem(world));
        }
    }

    private TreeViewItem MakeTreeViewItem(VaultObjectIdentifier obj)
    {
        var item = new TreeViewItem()
        {
            Header = obj.Name,
            Tag = obj.Id
        };
        
        item.MouseDoubleClick += ItemOnMouseDoubleClick;

        return item;
    }
    
    void ItemOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var treeViewItem = (TreeViewItem)sender;

        var name = (string)treeViewItem.Header;
        var id = (Guid)treeViewItem.Tag;

        var item = _tabControlData.FirstOrDefault(tab => tab.Id == id);
        if (item is not null)
        {
            TabControl.SelectedIndex = _tabControlData.IndexOf(item);
            return;
        }
        
        var tabItem = new HeaderOnlyTabItem()
        {
            Header = name,
            Id = id
        };

        _tabControlData.Add(tabItem);
        
        // TODO Add logic
    }

    private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TODO
    }

    private void TabItemRemove(object sender, MouseButtonEventArgs e)
    {
        _tabControlData.RemoveAt(TabControl.SelectedIndex);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        
        foreach (var tvi in Enum.GetValues<TreeViewItems>())
        {
            var collection = (TreeViewItem)TreeView.Items[(int)tvi]!;
            foreach (TreeViewItem item in collection.Items)
            {
                item.MouseDoubleClick -= ItemOnMouseDoubleClick;
            }
        }
    }
}