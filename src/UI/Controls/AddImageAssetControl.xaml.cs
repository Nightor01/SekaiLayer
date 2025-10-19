using System.Windows;
using SekaiLayer.Types;
using SekaiLayer.Utils;

namespace SekaiLayer.UI.Controls;

public partial class AddImageAssetControl : IValidatable
{
    public AddImageAssetControl()
    {
        InitializeComponent();
    }

    public bool Validate()
    {
        bool emptyName = string.IsNullOrEmpty(ImageName.Text);

        if (emptyName)
        {
            Dialogues.AddResourceError("Please fill in the image name");
            return false;
        }
        
        bool emptyFilePath = string.IsNullOrEmpty(FilePathDisplay.Text);

        if (emptyFilePath)
        {
            Dialogues.AddResourceError("Please fill in the file path to the image name");
            return false;
        }
        
        return true;
    }

    public object GetData()
    {
        return new ImportTypes.Image(
            ImageName.Text,
            FilePathDisplay.Text
        );
    }

    private void SelectPathOnClick(object sender, RoutedEventArgs e)
    {
        var folderDialog = FileSystemUtils.GetBasicOpenFileDialog(
            "Png file (.png)|*.png|" +
            "Jpeg file (.jpeg)|*.jpeg|" +
            "Jpeg file (.jpg)|*.jpg|" +
            "Bitmap file (.bmp)|*.bmp|" +
            "All files (*.*)|*.*"
            );
        
        var result = folderDialog.ShowDialog();
        if (result == null || !result.Value)
        {
            return;
        }
        
        FilePathDisplay.Text = folderDialog.FileName.Replace("\\", "/");
    }
}