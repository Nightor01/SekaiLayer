using System.Windows;

namespace SekaiLayer.UI.Windows;

public partial class SelectGroupDialog
{
    public int Index { get; private set; }
    public List<string> GroupNames { get; }
    
    public SelectGroupDialog(List<string> groupNames)
    {
        GroupNames = groupNames;
        
        InitializeComponent();
    }

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        int selctedIndex = ComboBox.SelectedIndex;

        if (selctedIndex == -1)
        {
            MessageBox.Show("Please select an asset group.", "Stop",
                MessageBoxButton.OK, MessageBoxImage.Stop
                );
            return;
        }

        Index = selctedIndex;
        DialogResult = true;
    }
}