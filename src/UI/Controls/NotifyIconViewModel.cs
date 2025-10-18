using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SekaiLayer.UI.Controls;

public partial class NotifyIconViewModel : ObservableObject
{
    public static event EventHandler ExitApplicationEvent = delegate { };
    public static event EventHandler ShowWindowEvent = delegate { };
    public static event EventHandler CreateNewVaultEvent = delegate { };

    [RelayCommand]
    private void ShowWindow()
    {
        ShowWindowEvent(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private void ExitApplication()
    {
        ExitApplicationEvent(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private void CreateNewVault()
    {
        CreateNewVaultEvent(this, EventArgs.Empty);
    }
}