using System.ComponentModel;
using System.Windows;
using SekaiLayer.Services;
using SekaiLayer.UI.Controls;

namespace SekaiLayer;

public class WindowEventArgs : EventArgs
{
    public required string WindowName { get; init; }
    public string? Path { get; init; }
}

public delegate void WindowEventHandler(object sender, WindowEventArgs e);

/// <summary>
/// Interaction logic for VaultManager.xaml
/// </summary>
public partial class VaultManager
{
    public event WindowEventHandler OpenWindowEvent = delegate { }; 
    public event WindowEventHandler CreatedNewWindowEvent = delegate { };
    public event WindowEventHandler RemoveWindowEvent = delegate { };
    public event WindowEventHandler DeleteWindowEvent = delegate { };
    
    private readonly FileManager _fileManager;
    
    private static readonly Size _vaultDisplaySize = new(150, 120);
    private static readonly Thickness _vaultDisplayMargin = new(10, 0, 10, 0);
    private const double _vaultDisplayBorderRadius = 10;
    private const double _vaultDisplayBorderWidth = 2;
    
    public VaultManager(FileManager fileManager)
    {
        _fileManager = fileManager;
        
        InitializeComponent();
        
        LoadVaults();
    }

    private void LoadVaults()
    {
        Vaults.Children.Clear();

        foreach (var entry in _fileManager.Entries)
        {
            var vault = new VaultDisplay(entry)
            {
                Width = _vaultDisplaySize.Width,
                Height = _vaultDisplaySize.Height,
                ToolTip = entry.Name,
                Margin = _vaultDisplayMargin,
                BorderRadius = _vaultDisplayBorderRadius,
                BorderWidth = _vaultDisplayBorderWidth,
            };
            
            vault.RemoveVaultEvent += VaultOnRemoveVaultEvent;
            
            Vaults.Children.Add(vault);
        }
    }

    private void VaultOnRemoveVaultEvent(object sender, RemoveVaultEventArgs e)
    {
        var display = (VaultDisplay)sender;
        
        display.RemoveVaultEvent -= VaultOnRemoveVaultEvent;
        
        // TODO: Decision logic (Remove / Delete)
        
        RemoveVaultWindow(sender, e);
    }

    private void OpenVaultWindow(object sender, RoutedEventArgs e)
    {
        // TODO: Selection logic
        string windowName = "name";
        
        OpenWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = windowName
        });
        
        Hide();
    }

    private void CreateNewVaultWindow(object sender, RoutedEventArgs e)
    {
        // TODO Creation logic
        string windowName = "name";
        string path = "path";

        CreatedNewWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = windowName,
            Path = path
        });
        
        Hide();
        LoadVaults();
    }

    private void RemoveVaultWindow(object sender, RoutedEventArgs e)
    {
        // TODO Creation logic
        string windowName = "name";
        
        RemoveWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = windowName
        });
        
        LoadVaults();
    }
    
    private void DeleteVaultWindow(object sender, RoutedEventArgs e)
    {
        // TODO Creation logic
        string windowName = "name";
        
        DeleteWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = windowName
        });
        
        LoadVaults();
    }
    
    protected override void OnClosing(CancelEventArgs e)
    {
        Hide();
        
        // We want to minimize to systray, not close the app
        e.Cancel = true;
    }

    private void Open()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void Exit()
    {
        TaskbarIcon.Dispose();
        Application.Current.Shutdown();
    }
    
    private void Open_OnClick(object sender, RoutedEventArgs e) => Open();
    private void Exit_OnClick(object sender, RoutedEventArgs e) => Exit();
    private void TaskbarIcon_OnTrayLeftMouseDoubleClick(object sender, RoutedEventArgs e) => Open();

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OpenButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}