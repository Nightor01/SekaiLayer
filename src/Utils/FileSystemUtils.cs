using Microsoft.Win32;

namespace SekaiLayer.Utils;

public static class FileSystemUtils
{
    public static string GetDialogStartPath()
    {
        string path;
        try
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
        catch (Exception ex) when (ex
            is ArgumentException
            or PlatformNotSupportedException
        ) {
            path = "/";
        }

        return path;
    }

    public static OpenFolderDialog GetBasicOpenFolderDialog()
    {
        return new OpenFolderDialog()
        {
            Multiselect = false,
            Title = "Choose a folder",
            InitialDirectory = GetDialogStartPath(),
        };
    }

    public static OpenFileDialog GetBasicOpenFileDialog(string filter)
    {
        return new OpenFileDialog()
        {
            Title = "Choose a file",
            Filter = filter,
            FilterIndex = 0,
        };
    }
}