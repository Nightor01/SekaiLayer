using System.ComponentModel;
using System.Windows;
using SekaiLayer.Types;
using SekaiLayer.UI.Windows;
using SekaiLayer.Utils;
using Point = System.Drawing.Point;

namespace SekaiLayer.UI.Controls;

public partial class AddTileSetAssetControl : IValidatable, INotifyPropertyChanged
{
    public static string Filter { get; } = "Png file (.png)|*.png|" 
                                         + "Bitmap file (.bmp)|*.bmp";
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _xCountValue = -1;
    private int _yCountValue = -1;
    private List<Point> _emptyTiles = [];

    private int _xCount
    {
        get => _xCountValue;
        set
        {
            _xCountValue = value;
            PropertyChanged?.Invoke(this, new(nameof(XCount)));
        }
    }
    
    private int _yCount
    {
        get => _yCountValue;
        set
        {
            _yCountValue = value;
            PropertyChanged?.Invoke(this, new(nameof(YCount)));
        }
    }

    public string XCount => _xCount == -1 ? "" : _xCount.ToString();
    public string YCount => _yCount == -1 ? "" : _yCount.ToString();
    
    public AddTileSetAssetControl()
    {
        InitializeComponent();
    }

    private void Configure_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new TileSetConfigurationDialog();
        
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _xCount = dialog.XCount;
        _yCount = dialog.YCount;
        _emptyTiles = dialog.EmptyTiles;
    }

    public bool Validate()
    {
        bool emptyName = string.IsNullOrEmpty(TileSetName.Text);

        if (emptyName)
        {
            Dialogues.AddResourceError("Please fill in the image name");
            return false;
        }
        
        bool emptyFilePath = string.IsNullOrEmpty(PathSelector.FilePath);

        if (emptyFilePath)
        {
            Dialogues.AddResourceError("Please fill in the file path to the image name");
            return false;
        }

        bool noXyCount = _xCount == -1 || _yCount == -1;

        if (noXyCount)
        {
            Dialogues.AddResourceError("Please configure the image count");
            return false;
        }

        return true;
    }

    public object GetData()
    {
        return new ImportTypes.TileSet(
            TileSetName.Text,
            PathSelector.FilePath,
            _xCount,
            _yCount,
            _emptyTiles
            );
    }
}