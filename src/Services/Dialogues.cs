using System.Windows;

namespace SekaiLayer.Services;

public static class Dialogues
{
    public static MessageBoxResult FileManagerError(string message)
    {
        return MessageBox.Show($"{Resources.AppTitle} "
                + $"experienced problems with file management.\nDo you want to continue?\n Error:" + message, 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}