using System.ComponentModel;
using System.Windows;
using SekaiLayer.Services;

namespace SekaiLayer;

public class OpenWindowEventArgs : EventArgs
{
    public required string WindowName { get; init; }
}

public delegate void OpenWindowEventHandler(object sender, OpenWindowEventArgs e);

/// <summary>
/// Interaction logic for VaultManager.xaml
/// </summary>
public partial class VaultManager
{
    public event OpenWindowEventHandler OpenWindowEvent = delegate { }; 
    public event OpenWindowEventHandler CreatedNewWindowEvent = delegate { };
    
    public VaultManager()
    {
        InitializeComponent();
    }

    private void OpenVaultWindow(object sender, RoutedEventArgs e)
    {
        // TODO: Selection logic
        string windowName = "name";
        
        OpenWindowEvent(sender, new OpenWindowEventArgs()
        {
            WindowName = windowName
        });
        
        Hide();
    }

    private void CreateNewVaultWindow(object sender, RoutedEventArgs e)
    {
        // TODO Creation logic
        string windowName = "name";

        CreatedNewWindowEvent(sender, new OpenWindowEventArgs()
        {
            WindowName = windowName
        });
        
        Hide();
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
}