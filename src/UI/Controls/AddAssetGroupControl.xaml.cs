using SekaiLayer.Types;
using SekaiLayer.Utils;

namespace SekaiLayer.UI.Controls;

public partial class AddAssetGroupControl : IValidatable
{
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

        Dialogues.AddResourceError("Please fill in the group name text field");

        return false;
    }

    public object GetData()
    {
        return new ImportTypes.AssetGroup(GroupName.Text);
    }
}