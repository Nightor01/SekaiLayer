using System.ComponentModel;
using System.Windows;
using H.NotifyIcon;
using SekaiLayer.Services;
using SekaiLayer.Types.Exceptions;
using SekaiLayer.UI.Controls;
using SekaiLayer.UI.Windows;

namespace SekaiLayer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public readonly VaultManager? VaultManager;
    private readonly Dictionary<string, VaultWindow> _openWindows = [];
    private readonly FileManager? _fileManager;
    private const string _settingsPath = "settings.json";
    private readonly TaskbarIcon? _notifyIcon;

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
        
        VaultManager = new(_fileManager);
        _notifyIcon = (TaskbarIcon)Current.FindResource("NotifyIcon")!;
        NotifyIconViewModel.ExitApplicationEvent += OnExitApplicationEvent;
        
        SubscribeVaultManagerEvents();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        if (_notifyIcon is not null && _notifyIcon.IsCreated)
        {
            _notifyIcon?.Dispose();
            NotifyIconViewModel.ExitApplicationEvent -= OnExitApplicationEvent;
        }
    }

    private void SubscribeVaultManagerEvents()
    {
        VaultManager!.OpenWindowEvent += OpenVaultWindow;
        VaultManager.CreatedNewWindowEvent += CreatedNewVaultWindow;
        VaultManager.RemoveWindowEvent += RemoveVaultWindow;
        VaultManager.DeleteWindowEvent += DeleteVaultWindow;
        VaultManager.Closing += VaultManagerOnClosing;
    }
    
    private void UnsubscribeVaultManagerEvents()
    {
        VaultManager!.OpenWindowEvent -= OpenVaultWindow;
        VaultManager.CreatedNewWindowEvent -= CreatedNewVaultWindow;
        VaultManager.RemoveWindowEvent -= RemoveVaultWindow;
        VaultManager.DeleteWindowEvent -= DeleteVaultWindow;
        VaultManager.Closing -= VaultManagerOnClosing;
    }
    
    private void OnExitApplicationEvent(object? sender, EventArgs e)
    {
        if (!_openWindows.Any())
        {
            Current.Shutdown();
            return;
        }
        
        MessageBoxResult result = MessageBox.Show(VaultManager!, "You have some vaults open now.\n"
            + "Are you certain you want to quit the application now?", "Warning",
            MessageBoxButton.YesNo, MessageBoxImage.Warning,  MessageBoxResult.No
            );

        if (result == MessageBoxResult.Yes)
        {
            Current.Shutdown();
        }
    }

    private void VaultManagerOnClosing(object? sender, CancelEventArgs e)
    {
        UnsubscribeVaultManagerEvents();
    }

    private void App_Startup(object sender, StartupEventArgs e)
    {
        // TODO: Just return?
        if (VaultManager is null || _fileManager is null || _notifyIcon is null)
            return;
        
        bool startMinimized = e.Args.Contains("--minimized") || _fileManager.AppSettings.StartMinimized;

        if (startMinimized)
        {
            VaultManager.Hide();
        }
        else
        {
            VaultManager!.Show();
        }
        
        _notifyIcon!.ForceCreate();
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

