using System.ComponentModel;
using System.IO;
using System.Windows;
using H.NotifyIcon;
using SekaiLayer.Services;
using SekaiLayer.Types;
using SekaiLayer.Types.Exceptions;
using SekaiLayer.UI.Controls;
using SekaiLayer.UI.Windows;
using SekaiLayer.Utils;

namespace SekaiLayer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly VaultSwitcher _vaultSwitcher;
    private readonly Dictionary<string, VaultWindow> _openWindows = [];
    private readonly FileManager _fileManager;
    private readonly string _settingsPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "settings.json"
        );
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
            _fileManager = null!;
            _vaultSwitcher = null!;
            Current.Shutdown();
            return;
        }
        
        _vaultSwitcher = new(_fileManager);
        _notifyIcon = (TaskbarIcon)Current.FindResource("NotifyIcon")!;
        SubscribeToNotifyIcon();
        
        SubscribeToVaultManagerEvents();
        
        // Can only be done when nothing failed
        Startup += App_Startup;
    }

    private void SubscribeToNotifyIcon()
    {
        NotifyIconViewModel.ExitApplicationEvent += OnExitApplicationEvent;
        NotifyIconViewModel.ShowWindowEvent += NotifyIconViewModelOnShowWindowEvent;
        NotifyIconViewModel.CreateNewVaultEvent += OnCreateNewVaultEvent;
    }

    private void UnsubscribeToNotifyIcon()
    {
        NotifyIconViewModel.ExitApplicationEvent -= OnExitApplicationEvent;
        NotifyIconViewModel.ShowWindowEvent -= NotifyIconViewModelOnShowWindowEvent;
        NotifyIconViewModel.CreateNewVaultEvent -= OnCreateNewVaultEvent;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        if (_notifyIcon is not null && _notifyIcon.IsCreated)
        {
            _notifyIcon?.Dispose();
            UnsubscribeToNotifyIcon();
        }
    }

    private void SubscribeToVaultManagerEvents()
    {
        _vaultSwitcher.OpenWindowEvent += OpenVaultWindow;
        _vaultSwitcher.RemoveWindowEvent += RemoveVaultWindow;
        _vaultSwitcher.DeleteWindowEvent += DeleteVaultWindow;
        _vaultSwitcher.Closing += VaultSwitcherOnClosing;
        _vaultSwitcher.CreateNewVaultEvent += OnCreateNewVaultEvent;
        _vaultSwitcher.RegisterNewVaultEvent += OnRegisterNewVaultEvent;
    }
    
    private void UnsubscribeToVaultManagerEvents()
    {
        _vaultSwitcher.OpenWindowEvent -= OpenVaultWindow;
        _vaultSwitcher.RemoveWindowEvent -= RemoveVaultWindow;
        _vaultSwitcher.DeleteWindowEvent -= DeleteVaultWindow;
        _vaultSwitcher.Closing -= VaultSwitcherOnClosing;
        _vaultSwitcher.CreateNewVaultEvent -= OnCreateNewVaultEvent;
        _vaultSwitcher.RegisterNewVaultEvent -= OnRegisterNewVaultEvent;
    }
    
    private void OnRegisterNewVaultEvent(object? sender, EventArgs e)
    {
        var dialog = new VaultSetupDialog();
        if (dialog.ShowDialog() == false)
            return;

        bool result = CreateNewVaultWindow(dialog.VaultConfig!, false);

        if (result)
        {
            _vaultSwitcher.Hide();
        }
    }
    
    void OnCreateNewVaultEvent(object? sender, EventArgs e)
    {
        var dialog = new VaultSetupDialog();
        if (dialog.ShowDialog() == false)
            return;

        var name = dialog.VaultConfig!.Name;
        // TODO resolve paths having both / and \
        var path = Path.Combine(dialog.VaultConfig!.Path, name);
        var encryption = dialog.VaultConfig!.Encryption;
        
        bool result = CreateNewVaultWindow(new VaultEntry()
        {
            Name = name,
            Path = path,
            Encryption = encryption
        }, true);

        if (result)
        {
            _vaultSwitcher.Hide();
        }
    }
    
    private void OnExitApplicationEvent(object? sender, EventArgs e)
    {
        if (!_openWindows.Any())
        {
            Current.Shutdown();
            return;
        }
        
        MessageBoxResult result = MessageBox.Show(_vaultSwitcher, "You have some vaults open now.\n"
            + "Are you certain you want to quit the application now?", "Warning",
            MessageBoxButton.YesNo, MessageBoxImage.Warning,  MessageBoxResult.No
            );

        if (result == MessageBoxResult.Yes)
        {
            Current.Shutdown();
        }
    }

    private void NotifyIconViewModelOnShowWindowEvent(object? sender, EventArgs e)
    {
        _vaultSwitcher.Show();
        _vaultSwitcher.WindowState = WindowState.Normal;
        _vaultSwitcher.Activate();
    }

    private void VaultSwitcherOnClosing(object? sender, CancelEventArgs e)
    {
        UnsubscribeToVaultManagerEvents();
    }

    private void App_Startup(object sender, StartupEventArgs e)
    {
        bool startMinimized = e.Args.Contains("--minimized") || _fileManager.AppSettings.StartMinimized;

        if (startMinimized)
        {
            _vaultSwitcher.Hide();
        }
        else
        {
            _vaultSwitcher.Show();
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
        else if(!StartVaultWindow(windowName))
        {
            return;
        }
        
        _vaultSwitcher.Hide();
    }

    private bool CreateNewVaultWindow(VaultEntry entry, bool createFiles)
    {
        string vaultName = entry.Name;
        
        try
        {
            if (createFiles)
            {
                VaultManager.PrepareVault(entry);
            }
            else if(VaultManager.CheckVaultFiles(entry))
            {
                throw new VaultManagerException("Vault files are not correct and vault cannot be registered.");
            }
            
            _fileManager.CreateVault(entry);
        }
        catch (FileManagerException ex)
        {
            Dialogues.FileManagerError(ex.Message);
            return false;
        }
        catch (VaultManagerException ex)
        {
            Dialogues.VaultManagerError(vaultName, ex.Message);
            return false;
        }
        
        return StartVaultWindow(vaultName);
    }

    private void ShowRemoveWindowMessage(string name)
    {
        MessageBox.Show(_vaultSwitcher, $"Please exit the working window `{name}` "
            + "before removing the vault", "Stop", MessageBoxButton.OK, MessageBoxImage.Stop
            );
    }

    private bool RemoveVaultWindowImplementation(string name)
    {
        if (_openWindows.ContainsKey(name))
        {
            ShowRemoveWindowMessage(name);
            return false;
        }
        
        try
        {
            _fileManager.RemoveVault(name);
        }
        catch (FileManagerException ex)
        {
            Dialogues.FileManagerError(ex.Message);
            return false;
        }

        return true;
    }
    
    private void RemoveVaultWindow(object sender, WindowEventArgs e) => RemoveVaultWindowImplementation(e.WindowName);

    private void DeleteVaultWindow(object sender, WindowEventArgs e)
    {
        var entry = _fileManager.GetEntry(e.WindowName);

        if (!RemoveVaultWindowImplementation(e.WindowName))
            return;
        
        try
        {
            VaultManager.RemoveVaultFiles(entry);
        }
        catch (VaultManagerException ex)
        {
            Dialogues.VaultManagerError(e.WindowName, ex.Message);
        }
    }

    private bool StartVaultWindow(string name)
    {
        VaultWindow window;
        try
        {
            var manager = new VaultManager(_fileManager.GetEntry(name));
            window = new(manager);
        }
        catch (FileManagerException e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        catch (VaultManagerException e)
        {
            Dialogues.VaultManagerError(name, e.Message);
            return false;
        }
        
        window.Closed += WindowOnClosed;

        window.Show();
        window.Activate();
        _openWindows[name] = window;

        return true;
    }

    private void WindowOnClosed(object? sender, EventArgs e)
    {
        var window = (VaultWindow)sender!;
        
        window.Closed -= WindowOnClosed;
        
        _openWindows.Remove(window.VaultName);
    }
}
