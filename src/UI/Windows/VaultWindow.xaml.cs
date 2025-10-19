using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SekaiLayer.Services;
using SekaiLayer.Types;
using SekaiLayer.UI.Controls;
using SekaiLayer.Types.Collections;
using SekaiLayer.Types.Exceptions;

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
        var assets = ((TreeViewItem)TreeView.Items[(int)TreeViewItems.Assets]!);
        assets.Items.Clear();
        foreach (var gp in _vaultManager.GetAssetGroups())
        {
            assets.Items.Add(MakeTreeViewItem(gp));
        }

        var worlds = ((TreeViewItem)TreeView.Items[(int)TreeViewItems.Worlds]!);
        worlds.Items.Clear();
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
            Content = GetContentControl(obj.Type)
        };

        _tabControlData.Add(tabItem);
        TabControl.SelectedIndex = 0;
    }

    private Control GetContentControl(VaultObjectIdentifier.ObjectType type)
    {
        return new Label() { Content = type.ToString() };
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
            case VaultObjectIdentifier.ObjectType.AssetGroup: AddAssetGroup(data); break;
            // TODO
            
            default: throw new NotImplementedException();
        }
        
        LoadAllTreeViewItems();
    }

    private void AddAssetGroup(object data)
    {
        var typedData = (AddAssetGroupControl.ReturnType)data;
        
        try
        {
            _vaultManager.AddAssetGroup(typedData.Name);
        }
        catch (VaultManagerException e)
        {
            MessageBox.Show("An error occured while trying to add asset group:\n" + e.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error
                );
        }
    }
}