using System.Windows;
using SekaiLayer.Services;
using SekaiLayer.Types.Exceptions;

namespace SekaiLayer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly VaultManager? _vaultManager;
    private readonly Dictionary<string, VaultWindow> _openWindows = [];
    private readonly FileManager? _fileManager;
    private const string _settingsPath = "settings.json";

    public App()
    {
        InitializeComponent();

        try
        {
            _fileManager = new(_settingsPath);
        }
        catch (FileManagerException e)
        {
            Dialogues.FileManagerError(e.Message);
            Current.Shutdown();
            return;
        }
        _vaultManager = new();

        // TODO: unsubscribe?
        _vaultManager.OpenWindowEvent += OpenVaultWindow;
        _vaultManager.CreatedNewWindowEvent += CreatedNewVaultWindow;
    }
    
    private void App_Startup(object sender, StartupEventArgs e)
    {
        var startMinimized = e.Args.Contains("--minimized");

        // TODO Make not appear upon start up
        _vaultManager!.Show();

        if (startMinimized)
        {
            _vaultManager.Hide();
        }
    }

    private void OpenVaultWindow(object sender, OpenWindowEventArgs e)
    {
        string windowName = e.WindowName;
        
        if (_openWindows.TryGetValue(windowName, out var window))
        {
            window.Activate();
        }
        else
        {
            StartVaultWindow(windowName);
        }
    }

    private void CreatedNewVaultWindow(object sender, OpenWindowEventArgs e)
    {
        StartVaultWindow(e.WindowName);
    }

    private void StartVaultWindow(string name)
    {
        VaultWindow window;
        try
        {
            var entry = _fileManager!.GetVaultEntry(name);
            window = new(_fileManager, entry);
        }
        catch (FileManagerException e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        window.Show();
        window.Activate();
        _openWindows[name] = window;
    }
}

