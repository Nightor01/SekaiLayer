using System.ComponentModel;
using System.Windows;

namespace SekaiLayer;

/// <summary>
/// Interaction logic for VaultManager.xaml
/// </summary>
public partial class VaultManager : Window
{
    public VaultManager()
    {
        InitializeComponent();
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