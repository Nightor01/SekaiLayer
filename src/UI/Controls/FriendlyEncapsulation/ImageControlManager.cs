using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SekaiLayer.Services;
using SekaiLayer.Types;
using SekaiLayer.Types.Data;
using SekaiLayer.Types.Exceptions;
using Image = System.Windows.Controls.Image;

namespace SekaiLayer.UI.Controls.FriendlyEncapsulation;

public class ImageControlManager
{
    private readonly Image _control = new Image();
    public Control Control => new ContentControl() { Content = _control };

    public ImageControlManager(VaultObjectIdentifier id, VaultManager vaultManager)
    {
        try
        {
            (AssetSettings, string)? pair = vaultManager.GetAssetSettings(id);

            if (pair is null)
            {
                throw new VaultManagerException("The image settins could not be loaded");
            }

            var (_, path) = pair.Value;

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path);
            bi.EndInit();
            _control.Source = bi;
        }
        catch (VaultManagerException e)
        {
            MessageBox.Show("Image loading failed.\n" + e.Message, "Error" ,
                MessageBoxButton.OK, MessageBoxImage.Error
                );
        }
    }
}