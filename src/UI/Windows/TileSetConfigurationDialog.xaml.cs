using System.Windows;
using System.Windows.Data;
using SekaiLayer.UI.Controls;
using SkiaSharp;
using Point = System.Drawing.Point;

namespace SekaiLayer.UI.Windows;

public partial class TileSetConfigurationDialog
{
    public int XCount = 0;
    public int YCount = 0;
    public List<Point> ExcludedTiles = [];
    
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
        nameof(Image),
        typeof(SKImage),
        typeof(TileSetConfigurationDialog),
        new PropertyMetadata(null)
        );

    public SKImage? Image
    {
        get => (SKImage)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }
    
    public TileSetConfigurationDialog(SKImage image)
    {
        InitializeComponent();
        
        // TODO figure out why it does not work without this
        TileSetConfigurationControl
            .SetBinding(TileSetConfigurationControl.ImageProperty,
                new Binding(nameof(Image)) { Source = this }
                );
        
        Image = image;
    }
}