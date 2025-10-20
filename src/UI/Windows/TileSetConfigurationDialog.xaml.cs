using System.ComponentModel;
using System.Windows;
using Point = System.Drawing.Point;

namespace SekaiLayer.UI.Windows;

public partial class TileSetConfigurationDialog : Window
{
    public int XCount = 0;
    public int YCount = 0;
    public List<Point> ExcludedTiles = [];
    
    public TileSetConfigurationDialog()
    {
        InitializeComponent();
    }
}