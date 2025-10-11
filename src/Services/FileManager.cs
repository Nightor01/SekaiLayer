namespace SekaiLayer.Services;

public class FileManager
{
    private string _settingsPath;

    public FileManager(string settingsPath)
    {
        _settingsPath = settingsPath;
    }

    public VaultWindow GetVaultWindow(string name)
    {
        throw new NotImplementedException();
    }
}