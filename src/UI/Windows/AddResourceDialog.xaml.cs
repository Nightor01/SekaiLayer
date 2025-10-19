using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SekaiLayer.Types;
using SekaiLayer.Types.Attributes;

namespace SekaiLayer.UI.Windows;

public partial class AddResourceDialog
{
    public ObservableCollection<string> ObjectTypes { get; } = VaultObjectNames();
    
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
        
        ContentHolder.Content = new Label() { Content = objectType.ToString() };
    }

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = Validate();
        
        Close();
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        
        Close();
    }
}