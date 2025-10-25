using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SekaiLayer.Services;
using SekaiLayer.Types;
using SekaiLayer.UI.Controls;
using SekaiLayer.Types.Collections;
using SekaiLayer.Types.Exceptions;
using SekaiLayer.UI.Controls.FriendlyEncapsulation;

namespace SekaiLayer.UI.Windows;

public partial class VaultWindow
{
    private readonly VaultManager _vaultManager;
    public string VaultName => _vaultManager.VaultName;
    private readonly ReverseObservableCollection<CustomTabItem> _tabControlData = [];

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

        LoadAllTreeViewItems();
    }

    private void LoadAllTreeViewItems()
    {
        var groups = GetSubTree(TreeViewItems.Assets);
        groups.Items.Clear();
        foreach (var gp in _vaultManager.GetAssetGroups())
        {
            var treeViewItem = MakeTreeViewItem(gp);

            var assets = GetAssetsFromGroup(gp);
            foreach (var asset in assets)
            {
                treeViewItem.Items.Add(MakeTreeViewItem(asset));
            }
            
            groups.Items.Add(treeViewItem);
        }

        var worlds = GetSubTree(TreeViewItems.Worlds);
        worlds.Items.Clear();
        foreach (var world in _vaultManager.GetWorlds())
        {
            worlds.Items.Add(MakeTreeViewItem(world));
        }
    }

    private List<VaultObjectIdentifier> GetAssetsFromGroup(VaultObjectIdentifier group)
    {
        List<VaultObjectIdentifier> assets;

        try
        {
            assets = _vaultManager.GetAssetsFromGroup(group);
        }
        catch (VaultManagerException e)
        {
            MessageBox.Show($"There was an error with reading from group – {group.Name}. Skipping...\n" 
                + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error
                );
            return [];
        }

        return assets;
    }

    private TreeViewItem MakeTreeViewItem(VaultObjectIdentifier obj)
    {
        var item = new TreeViewItem()
        {
            Header = obj.Name,
            Tag = obj
        };
        
        item.MouseDoubleClick += ItemOnMouseDoubleClick;

        return item;
    }
    
    void ItemOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var treeViewItem = (TreeViewItem)sender;
        
        var obj = (VaultObjectIdentifier)treeViewItem.Tag;

        if (!obj.IsContentOpenable())
            return;

        var item = _tabControlData.FirstOrDefault(tab => tab.Id == obj.Id);
        if (item is not null)
        {
            TabControl.SelectedIndex = _tabControlData.IndexOf(item);
            return;
        }
        
        var tabItem = new CustomTabItem()
        {
            Header = obj.Name,
            Id = obj.Id,
            Content = GetContentControl(obj)
        };

        _tabControlData.Add(tabItem);
        TabControl.SelectedIndex = 0;
    }

    private Control GetContentControl(VaultObjectIdentifier id)
    {
        return id.Type switch
        {
            VaultObjectIdentifier.ObjectType.TileSet
                => new TileSetControlManager(id, _vaultManager).Control,
            VaultObjectIdentifier.ObjectType.Image
                => new ImageControlManager(id, _vaultManager).Control,
            
            // TODO
            _ => new Label() { Content = id.Type.ToString() } 
        };
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

    private void AddResourceOnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new AddResourceDialog();
        
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var (type, data) = dialog.Data!.Value;

        switch (type)
        {
            case VaultObjectIdentifier.ObjectType.AssetGroup: AddAssetGroup(data); return;
            case VaultObjectIdentifier.ObjectType.Image: AddImageAsset(data); break;
            case VaultObjectIdentifier.ObjectType.TileSet: AddTileSetAsset(data); break;
            case VaultObjectIdentifier.ObjectType.World: return;
            // TODO
            
            default: throw new NotImplementedException();
        }
        
        // LoadAllTreeViewItems();
    }

    private void AddAssetGroup(object data)
    {
        var typedData = (ImportTypes.AssetGroup)data;

        try
        {
            _vaultManager.AddAssetGroup(typedData.Name);
        }
        catch (VaultManagerException e)
        {
            MessageBox.Show("An error occured while trying to add asset group:\n" + e.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error
            );
            return;
        }

        var subTree = GetSubTree(TreeViewItems.Assets);
        VaultObjectIdentifier group = _vaultManager
            .GetAssetGroups()
            .First(x => x.Name == typedData.Name);
        subTree.Items.Add(MakeTreeViewItem(group));
    }

    private void AddAssetToTreeView(TreeViewItem tvi, VaultObjectIdentifier group, string name)
    {
        VaultObjectIdentifier asset = _vaultManager
            .GetAssetsFromGroup(group)
            .First(x => x.Name == name);
        tvi.Items.Add(MakeTreeViewItem(asset));
    }
    
    private void AddImageAsset(object data)
    {
        var typedData = (ImportTypes.Image)data;

        int index = GroupSelection();

        if (index == -1)
            return;

        var tvi = (TreeViewItem)GetSubTree(TreeViewItems.Assets).Items[index]!;
        var group = (VaultObjectIdentifier)tvi.Tag; 

        try
        {
            _vaultManager.AddImage(group, typedData);
        }
        catch (VaultManagerException e)
        {
            MessageBox.Show("There was an error with adding this image.\n" + e.Message, 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error
                );
            return;
        }

        AddAssetToTreeView(tvi, group, typedData.Name);
    }

    private void AddTileSetAsset(object data)
    {
        var typedData = (ImportTypes.TileSet)data;

        int index = GroupSelection();

        if (index == -1)
            return;

        var tvi = (TreeViewItem)GetSubTree(TreeViewItems.Assets).Items[index]!;
        var group = (VaultObjectIdentifier)tvi.Tag; 

        try
        {
            _vaultManager.AddTileSet(group, typedData);
        }
        catch (VaultManagerException e)
        {
            MessageBox.Show("There was an error with adding this tileset.\n" + e.Message, 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error
            );
        }
        
        AddAssetToTreeView(tvi, group, typedData.Name);
    }

    private int GroupSelection()
    {
        var names = _vaultManager
            .GetAssetGroups()
            .Select(x => x.Name)
            .ToList();
        
        var dialog = new SelectGroupDialog(names);

        if (dialog.ShowDialog() != true)
        {
            MessageBox.Show("Asset import was canceled", "Information",
                MessageBoxButton.OK, MessageBoxImage.Information
            );
            return -1;
        }

        return dialog.Index;
    }

    private TreeViewItem GetSubTree(TreeViewItems tvi)
    {
        return (TreeViewItem)TreeView.Items[(int)tvi]!;
    }

    private void AddExisting_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void AddDefault_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}