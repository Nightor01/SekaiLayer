using System.ComponentModel;
using System.Windows;
using SekaiLayer.Services;
using SekaiLayer.Types.Exceptions;
using SekaiLayer.UI.Windows;

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
        _vaultManager = new(_fileManager);
        
        SubscribeVaultManagerEvents();
    }

    private void SubscribeVaultManagerEvents()
    {
        _vaultManager!.OpenWindowEvent += OpenVaultWindow;
        _vaultManager.CreatedNewWindowEvent += CreatedNewVaultWindow;
        _vaultManager.RemoveWindowEvent += RemoveVaultWindow;
        _vaultManager.DeleteWindowEvent += DeleteVaultWindow;
        _vaultManager.Closing += VaultManagerOnClosing;
    }

    private void UnsubscribeVaultManagerEvents()
    {
        _vaultManager!.OpenWindowEvent -= OpenVaultWindow;
        _vaultManager.CreatedNewWindowEvent -= CreatedNewVaultWindow;
        _vaultManager.RemoveWindowEvent -= RemoveVaultWindow;
        _vaultManager.DeleteWindowEvent -= DeleteVaultWindow;
        _vaultManager.Closing -= VaultManagerOnClosing;
    }

    private void VaultManagerOnClosing(object? sender, CancelEventArgs e)
    {
        UnsubscribeVaultManagerEvents();
    }

    private void App_Startup(object sender, StartupEventArgs e)
    {
        // TODO: Just return?
        if (_vaultManager is null || _fileManager is null)
            return;
        
        bool startMinimized = e.Args.Contains("--minimized") || _fileManager.AppSettings.StartMinimized;

        // TODO: Make not appear upon start up
        _vaultManager!.Show();

        if (startMinimized)
        {
            _vaultManager.Hide();
        }
    }

    private void OpenVaultWindow(object sender, WindowEventArgs e)
    {
        string windowName = e.WindowName;
        
        if (_openWindows.TryGetValue(windowName, out var window))
        {
            window.Show();
            window.Activate();
        }
        else
        {
            StartVaultWindow(windowName);
        }
    }

    private void CreatedNewVaultWindow(object sender, CreateVaultEventArgs e)
    {
        try
        {
            _fileManager!.CreateVaultWindow(e.Entry);
        }
        catch (FileManagerException ex)
        {
            Dialogues.FileManagerError(ex.Message);
            return;
        }
        
        StartVaultWindow(e.Entry.Name);
    }

    private void RemoveVaultWindow(object sender, WindowEventArgs e)
    {
        try
        {
            _fileManager!.RemoveVaultWindow(e.WindowName);
        }
        catch (FileManagerException ex)
        {
            Dialogues.FileManagerError(ex.Message);
        }
    }

    private void DeleteVaultWindow(object sender, WindowEventArgs e)
    {
        try
        {
            _fileManager!.DeleteVaultWindow(e.WindowName);
        }
        catch (FileManagerException ex)
        {
            Dialogues.FileManagerError(ex.Message);
        }
    }

    private void StartVaultWindow(string name)
    {
        VaultWindow window;
        try
        {
            window = new(_fileManager!, name);
        }
        catch (FileManagerException e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        
        window.Closed += WindowOnClosed;

        window.Show();
        window.Activate();
        _openWindows[name] = window;
    }

    private void WindowOnClosed(object? sender, EventArgs e)
    {
        var window = (VaultWindow)sender!;
        
        window.Closed -= WindowOnClosed;
        
        _openWindows.Remove(window.VaultName);
    }
}

