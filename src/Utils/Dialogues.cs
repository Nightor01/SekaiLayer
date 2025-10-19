using System.Windows;

namespace SekaiLayer.Utils;

public static class Dialogues
{
    public static void FileManagerError(string message)
    {
        MessageBox.Show($"{Resources.AppTitle} " 
            + $"experienced problems with file management.\nError: " + message, 
            "Error", MessageBoxButton.OK, MessageBoxImage.Error
            );
    }

    public static void VaultManagerError(string vaultName, string message)
    {
        MessageBox.Show($"{Resources.AppTitle} – `{vaultName}` " 
            + $"experienced problems with vault's file management.\nError: " + message, 
            "Error", MessageBoxButton.OK, MessageBoxImage.Error
        );
    }

    public static void AddResourceError(string message)
    {
        MessageBox.Show(message, "Stop", MessageBoxButton.OK, MessageBoxImage.Stop);
    }
}