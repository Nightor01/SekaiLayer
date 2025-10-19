using System.Windows;
using SekaiLayer.Types;

namespace SekaiLayer.UI.Controls;

public partial class AddAssetGroupControl : IValidatable
{
    public record ReturnType(string Name);
    
    public AddAssetGroupControl()
    {
        InitializeComponent();
    }

    public bool Validate()
    {
        bool empty = string.IsNullOrEmpty(GroupName.Text);

        if (!empty)
        {
            return true;
        }

        MessageBox.Show("Please fill in the group name text field", "Stop",
            MessageBoxButton.OK, MessageBoxImage.Stop 
            );

        return false;
    }

    public object GetData()
    {
        return new ReturnType(GroupName.Text);
    }
}