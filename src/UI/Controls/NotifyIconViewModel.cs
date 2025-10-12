using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace SekaiLayer.UI.Controls;

public partial class NotifyIconViewModel : ObservableObject
{
    // TODO: Must it be static?
    public static event EventHandler ExitApplicationEvent = delegate { };

    [RelayCommand]
    private void ShowWindow()
    {
        var app = (App)Application.Current;

        if (app.VaultManager is null)
            return;
        
        app.VaultManager.Show();
        app.VaultManager.WindowState = WindowState.Normal;
        app.VaultManager.Activate();
    }
    
    [RelayCommand]
    private void ExitApplication()
    {
        ExitApplicationEvent(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private void CreateNewVault()
    {
        var app = (App)Application.Current;

        if (app.VaultManager is null)
            return;

        // TODO: This is a strange way to go about it
        app.VaultManager.CreateNewVault(this, new RoutedEventArgs()
        {
            Source = this
        });
    }
}