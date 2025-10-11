using System.ComponentModel;
using System.Windows;
using SekaiLayer.Services;

namespace SekaiLayer;

public class WindowEventArgs : EventArgs
{
    public required string WindowName { get; init; }
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
    
    public VaultManager(FileManager fileManager)
    {
        _fileManager = fileManager;
        
        InitializeComponent();
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

        CreatedNewWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = windowName
        });
        
        Hide();
    }

    private void RemoveVaultWindow(object sender, RoutedEventArgs e)
    {
        // TODO Creation logic
        string windowName = "name";
        
        RemoveWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = windowName
        });
    }
    
    private void DeleteVaultWindow(object sender, RoutedEventArgs e)
    {
        // TODO Creation logic
        string windowName = "name";
        
        DeleteWindowEvent(sender, new WindowEventArgs()
        {
            WindowName = windowName
        });
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

    private void CreateButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OpenButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}