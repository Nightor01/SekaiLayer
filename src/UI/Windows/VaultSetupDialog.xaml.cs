using Microsoft.Win32;
using System.Windows;
using SekaiLayer.Types;
using SekaiLayer.Types.Data;

namespace SekaiLayer.UI.Windows;

public partial class VaultSetupDialog
{
    public VaultEntry? VaultConfig { get; private set; } = null;
    
    public VaultSetupDialog()
    {
        InitializeComponent();

        for (int i = (int)VaultEntry.EncryptionType.None; i < Enum.GetValues<VaultEntry.EncryptionType>().Length; ++i)
        {
            EncryptionType.Items.Add((VaultEntry.EncryptionType)i);
        }

        EncryptionType.SelectedItem = VaultEntry.EncryptionType.None;
    }

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        var success = VaultSetup();
        if (success)
        {
            DialogResult = true;
            return;
        }
        
        var result = MessageBox.Show("Vault setup failed\nClose", "Error",
            MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            DialogResult = success;
        }
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private bool VaultSetup()
    {
        VaultConfig = new VaultEntry()
        {
            Encryption = (VaultEntry.EncryptionType)EncryptionType.SelectedItem,
            Name = VaultName.Text,
            Path = (string)VaultPath.Content
        };
        
        return !string.IsNullOrEmpty(VaultConfig.Path) && !string.IsNullOrEmpty(VaultConfig.Name);
    }

    private void SetFolder_OnClick(object sender, RoutedEventArgs e)
    {
        var folderDialog = Utils.FileSystemUtils.GetBasicOpenFolderDialog();
        
        var result = folderDialog.ShowDialog();
        if (result == null || !result.Value)
        {
            return;
        }
        
        VaultPath.ToolTip = VaultPath.Content = folderDialog.FolderName.Replace("\\", "/");
    }
}