using System.Windows;

namespace SekaiLayer.Services;

public static class Dialogues
{
    public static void FileManagerError(string message)
    {
        MessageBox.Show($"{Resources.AppTitle} " 
            + $"experienced problems with file management.\nError:" + message, 
            "Error", MessageBoxButton.OK, MessageBoxImage.Error
            );
    }
}