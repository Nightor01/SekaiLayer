using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using SekaiLayer.UI.Controls;
using SkiaSharp;

namespace SekaiLayer.UI.Windows;

public partial class TileSetConfigurationDialog
{
    public int XCount { get; private set; } = -1;
    public int YCount { get; private set; } = -1;
    public List<Rect> ExcludedTiles { get; private set; } = [];
    
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
        nameof(Image),
        typeof(SKImage),
        typeof(TileSetConfigurationDialog),
        new PropertyMetadata(null)
        );

    public SKImage? Image
    {
        get => (SKImage)GetValue(ImageProperty);
        init => SetValue(ImageProperty, value);
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
        
        TileSetConfigurationControl.ApplyOk += TileSetConfigurationControlOnApplyOk;
        TileSetConfigurationControl.ApplyCancel += TileSetConfigurationControlOnApplyCancel;
    }

    private void TileSetConfigurationControlOnApplyCancel(object? sender, EventArgs e)
    {
        DialogResult = false;
    }

    private void TileSetConfigurationControlOnApplyOk(object? sender, EventArgs e)
    {
        XCount = TileSetConfigurationControl.XCount;
        YCount = TileSetConfigurationControl.YCount;
        ExcludedTiles = TileSetConfigurationControl.ExcludedTiles.ToList();

        DialogResult = true;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        
        TileSetConfigurationControl.ApplyOk -= TileSetConfigurationControlOnApplyOk;
        TileSetConfigurationControl.ApplyCancel -= TileSetConfigurationControlOnApplyCancel;
    }
}