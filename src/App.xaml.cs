using System.Windows;

namespace SekaiLayer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly VaultManager _vaultManager;

    public App()
    {
        InitializeComponent();
        
        _vaultManager = new ();
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
}

