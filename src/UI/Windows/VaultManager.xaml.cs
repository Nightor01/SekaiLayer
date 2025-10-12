using System.ComponentModel;
using System.Windows;
using SekaiLayer.Services;
using SekaiLayer.Types;
using SekaiLayer.UI.Controls;

namespace SekaiLayer.UI.Windows;

public class WindowEventArgs : EventArgs
{
    public required string WindowName { get; init; }
}

public delegate void WindowEventHandler(object sender, WindowEventArgs e);

public class CreateVaultEventArgs : EventArgs
{
    public required VaultEntry Entry { get; init; }
}

public delegate void CreateVaultEventHandler(object sender, CreateVaultEventArgs e);

/// <summary>
/// Interaction logic for VaultManager.xaml
/// </summary>
public partial class VaultManager
{
    public event CreateVaultEventHandler CreatedNewWindowEvent = delegate { };
    public event WindowEventHandler OpenWindowEvent = delegate { }; 
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
            var display = new VaultDisplay(entry)
            {
                Width = _vaultDisplaySize.Width,
                Height = _vaultDisplaySize.Height,
                ToolTip = entry.Name,
                Margin = _vaultDisplayMargin,
                BorderRadius = _vaultDisplayBorderRadius,
                BorderWidth = _vaultDisplayBorderWidth,
            };
            
            Subscribe(display);
            
            Vaults.Children.Add(display);
        }
    }

    private void VaultOnRemoveVaultEvent(object sender, RemoveVaultEventArgs e)
    {
        var display = (VaultDisplay)sender;
        
        MessageBoxResult result = MessageBox.Show("Do you also want to delete the files?",
             $"{SekaiLayer.Resources.AppTitle}", MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
             MessageBoxResult.No 
             );

        if (result == MessageBoxResult.Cancel) return;
        
        Unsubscribe(display);

        if (result == MessageBoxResult.No)
        {
            RemoveVault(sender, e);
            return;
        }
        
        result = MessageBox.Show("Are you sure? The files will be deleted permanently.\n"
            + "(And that is a very long time)", "Warning",  MessageBoxButton.YesNo,
            MessageBoxImage.Warning, MessageBoxResult.No
            );
        
        if (result == MessageBoxResult.No)
        {
            RemoveVault(sender, e);
            return;
        }
        
        DeleteVault(sender, e);
    }

    private void OpenVaultWindow(object sender, RoutedEventArgs e)
    {
        var display = (VaultDisplay)sender;
        
        OpenWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = display.VaultName
        });
        
        Hide();
    }

    private void CreateNewVault(object sender, RoutedEventArgs e)
    {
        RegisterVault(sender, e);
        
        // TODO: Prepare vault
    }

    private void RegisterVault(object sender, RoutedEventArgs e)
    {
        var dialog = new VaultSetupDialog();
        dialog.ShowDialog();

        if (dialog.DialogResult == false || dialog.VaultName is null)
            return;

        CreatedNewWindowEvent(sender, new CreateVaultEventArgs()
        {
            Entry = dialog.VaultConfig!
        });
        
        Hide();
        LoadVaults();
    }

    private void RemoveVault(object sender, RoutedEventArgs _)
    {
        var display = (VaultDisplay)sender;
        
        RemoveWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = display.VaultName
        });
        
        LoadVaults();
    }
    
    private void DeleteVault(object sender, RoutedEventArgs _)
    {
        var display = (VaultDisplay)sender;
        
        DeleteWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = display.VaultName
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

    private void Subscribe(VaultDisplay display)
    {
        display.RemoveVaultEvent += VaultOnRemoveVaultEvent;
        display.MouseDoubleClick += OpenVaultWindow;
    }
    
    private void Unsubscribe(VaultDisplay display)
    {
        display.RemoveVaultEvent -= VaultOnRemoveVaultEvent;
        display.MouseDoubleClick -= OpenVaultWindow;
    }
    
    private void Open_OnClick(object sender, RoutedEventArgs e) => Open();
    private void Exit_OnClick(object sender, RoutedEventArgs e) => Exit();
    private void TaskbarIcon_OnTrayLeftMouseDoubleClick(object sender, RoutedEventArgs e) => Open();

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        new SettingsDialog(_fileManager).ShowDialog();
    }
}