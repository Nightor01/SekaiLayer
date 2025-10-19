using System.Windows;

namespace SekaiLayer.UI.Controls;

public partial class PathSelector
{
    public string FilePath => FilePathDisplay.Text;
    
    public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
        nameof(Filter),
        typeof(string),
        typeof(PathSelector),
        new PropertyMetadata("All files (*.*)|*.*")
        );

    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    public PathSelector()
    {
        InitializeComponent();
    }
    
    private void SelectPathOnClick(object sender, RoutedEventArgs e)
    {
        var folderDialog = Utils.FileSystemUtils.GetBasicOpenFileDialog(Filter);
        
        var result = folderDialog.ShowDialog();
        if (result == null || !result.Value)
        {
            return;
        }
        
        FilePathDisplay.Text = folderDialog.FileName.Replace("\\", "/");
    }
}