using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SekaiLayer.Types;
using SekaiLayer.Types.Attributes;
using SekaiLayer.UI.Controls;

namespace SekaiLayer.UI.Windows;

public partial class AddResourceDialog
{
    public ObservableCollection<string> ObjectTypes { get; } = VaultObjectNames();
    private const double _errorLabelThickness = 5; 
    public (VaultObjectIdentifier.ObjectType, object)? Data { get; private set; }
    
    public AddResourceDialog()
    {
        InitializeComponent();
    }
    
    private bool Validate()
    {
        if (ObjectType.SelectedIndex != -1 && ContentHolder.Content is IValidatable validatable)
        {
            return validatable.Validate();
        }
        
        MessageBox.Show("Please select an object type", "Stop",
            MessageBoxButton.OK, MessageBoxImage.Stop
        );
        
        return false;
    }
    
    private static ObservableCollection<string> VaultObjectNames()
    {
        var values = typeof(VaultObjectIdentifier.ObjectType)
            .GetFields()
            .Select(f =>
            {
                var attr = Attribute.GetCustomAttribute(f, typeof(VaultObjectNameAttribute));

                return attr is null
                    ? null
                    : ((VaultObjectNameAttribute)attr!).Name;
            })
            .Where(v => v is not null)!
            .ToList<string>();
        return new(values);
    }

    private void ObjectType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int selectedIndex = ObjectType.SelectedIndex;

        if (selectedIndex == -1)
        {
            ContentHolder.Content = new();
            return;
        }
        
        var objectType = (VaultObjectIdentifier.ObjectType)selectedIndex;

        ContentHolder.Content = objectType switch
        {
            VaultObjectIdentifier.ObjectType.AssetGroup => new AddAssetGroupControl(),
            VaultObjectIdentifier.ObjectType.Image => new AddImageAssetControl(),
            VaultObjectIdentifier.ObjectType.TileSet => new AddTileSetAssetControl(),
            _ => new TextBlock()
            {
                Text = "error: this operation is not yet supported",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(_errorLabelThickness),
                Foreground = Brushes.Red
            }
        };
    }

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        bool result = Validate();

        if (result != true)
            return;

        var data = (ContentHolder.Content as IValidatable)!.GetData();
        var type = (VaultObjectIdentifier.ObjectType)ObjectType.SelectedIndex;
        Data = (type, data);

        DialogResult = result;
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}