using System.Windows;
using SekaiLayer.Services;

namespace SekaiLayer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly VaultManager _vaultManager;
    private readonly Dictionary<string, VaultWindow> _openWindows = [];
    private readonly FileManager _fileManager;
    private const string _settingsPath = "settings.json";

    public App()
    {
        InitializeComponent();
        
        _vaultManager = new();
        _fileManager = new(_settingsPath);

        // TODO: unsubscribe?
        _vaultManager.OpenWindowEvent += OpenVaultWindow;
    }
    
    private void App_Startup(object sender, StartupEventArgs e)
    {
        var startMinimized = e.Args.Contains("--minimized");

        // TODO Make not appear upon start up
        _vaultManager.Show();

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

    private void StartVaultWindow(string name)
    {
        var window = _fileManager.GetVaultWindow(name);

        window.Show();
        window.Activate();
        _openWindows[name] = window;
    }
}

