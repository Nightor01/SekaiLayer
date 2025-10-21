using System.Media;
using System.Windows;
using System.Windows.Input;
using SekaiLayer.Types.Collections;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SekaiLayer.UI.Controls;

public partial class TileSetConfigurationControl
{
    public int XCount => XCountNud.Value ?? -1;
    public int YCount => YCountNud.Value ?? -1;
    public ObservableSet<Rect> ExcludedPoints { get; } = [];
    
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
        nameof(Image),
        typeof(SKImage),
        typeof(TileSetConfigurationControl),
        new PropertyMetadata(null)
        );

    public SKImage? Image
    {
        get => (SKImage)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    private static readonly SKPaint _linePaint = new()
    {
        Color = SKColors.Red,
        StrokeWidth = 2,
        Style = SKPaintStyle.Stroke
    };
    
    private static readonly SKPaint _rectPaint = new()
    {
        Color = SKColors.Blue,
        StrokeWidth = 2,
        Style = SKPaintStyle.Stroke
    };
    
    public TileSetConfigurationControl()
    {
        InitializeComponent();

        XCountNud.Value = 1;
        YCountNud.Value = 1;
        PerfectFit.Visibility = Visibility.Visible;
    }
    
    private void Canvas_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (Image is null)
        {
            return;
        }
        
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        var rect = GetDestinationRectangle(e.Info.Rect);
        
        canvas.DrawImage(Image, rect);

        int? xCount = XCountNud.Value;
        int? yCount = YCountNud.Value;

        if (xCount is null || yCount is null)
        {
            return;
        }
        
        int xRatio = Image.Width / xCount.Value;
        double xDifference = (rect.Width / Image.Width) * xRatio;
        int yRatio = Image.Height / yCount.Value;
        double yDifference = (rect.Height / Image.Height) * yRatio;

        for (int x = 0; x <= xCount.Value; ++x)
        {
            float xPos = (float)(x * xDifference);
            canvas.DrawLine(xPos, 0, xPos, (float)(yCount * yDifference), _linePaint);
        }

        for (int y = 0; y <= yCount.Value; ++y)
        {
            float yPos = (float)(y * yDifference);
            canvas.DrawLine(0, yPos, (float)(xCount * xDifference), yPos, _linePaint);
        }
        
        canvas.DrawRect(rect, _rectPaint);
    }

    private SKRect GetDestinationRectangle(SKRectI canvas) => new(
            0, 0,
            (float)double.Min(canvas.Height * Image!.Width / (double)Image.Height, canvas.Width),
            (float)double.Min(canvas.Width * Image.Height / (double)Image.Width, canvas.Height)
            );

    private void AddExclusion_OnClick(object sender, RoutedEventArgs e)
    {
        string text = ExclusionBox.Text;

        if (!TryAddExclusion(text))
        {
            SystemSounds.Exclamation.Play();
        }
    }

    private bool TryAddExclusion(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        if (!text.Contains('-'))
        {
            Point? point = GetExcludedPoint(text);

            if (point is null)
            {
                return false;
            }
            
            ExcludedPoints.Add(
                new(point.Value, new Size(0, 0))
                );
            return true;
        }
        
        (Point, Point)? range = GetExclusionRange(text);
        
        if (range is null)
        {
            return false;
        }
        
        ExcludedPoints.Add(
            new(range.Value.Item1, range.Value.Item2)
            );

        return true;
    }

    private (Point, Point)? GetExclusionRange(string text)
    {
        string[] points = text.Split('-');
        Point?[] values = points
            .Select(GetExcludedPoint)
            .ToArray();

        if (values.Length != 2 || values.Contains(null))
        {
            return null;
        }

        return (values[0]!.Value, values[1]!.Value);
    }

    private Point? GetExcludedPoint(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        
        if (text[0] != '(' || text[^1] != ')')
        {
            return null;
        }
        
        string[] numbers = text
            .Substring(1, text.Length - 2)
            .Split(';');
        
        int[] values = numbers
            .Select(x =>
            {
                if (!int.TryParse(x, out int value) || value < 0 || value >= XCountNud.Value)
                    return -1;
                return value;
            })
            .ToArray();

        if (values.Length != 2 || values.Contains(-1))
        {
            return null;
        }
        
        return new Point(values[0], values[1]);
    }

    private void ExclusionBox_OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (!TryAddExclusion(ExclusionBox.Text))
        {
            SystemSounds.Exclamation.Play();
            return;
        }

        if (!Keyboard.IsKeyDown(Key.LeftShift))
        {
            ExclusionBox.Text = string.Empty;
        }
    }

    private void RemoveExclusion_OnClick(object sender, RoutedEventArgs e)
    {
        ExcludedPoints.ExceptWith(ExclusionListDisplay.SelectedItems.AsQueryable().Cast<Rect>());
    }

    private void ClearExclusion_OnClick(object sender, RoutedEventArgs e)
    {
        ExcludedPoints.Clear();
    }

    private void Help_OnClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("""
                        By setting the `x` and `y` sample counts, you can set the tiling of the chosen tileset.
                        
                        At the exclusion part of the dialog, you can set tiles, that should not be included in
                        the tileset (for example tiles, that are completely empty). This can be done by entering
                        individual points inside the textbox in the format `(x;y)`, by entering rectangular
                        regions in the format `(x1;y1)-(x2;y2)`, or by clicking on the `Manual` toggle button
                        and selecting the tiles with a mouse.
                        
                        Textbox selection:
                        After entering the desired point or region, you can press enter to add it to exclusions
                        and clear the textbox, or you can press shift+enter to add it to exclusions but not clear
                        the textbox. Alternatively, you can use the `Add` button for that.
                        
                        Manual selection:
                        By clicking you can select or deselect a tile. By draging the mouse you can select or
                        deselect a whole region.
                        
                        Removal of exclusions:
                        That can be done manually or within the exclusion display box. By selecting an exclusion
                        and pressing `Remove`, you can remove it. By pressing clear, you can clear all entered
                        exclusions.
                        """, "Help", MessageBoxButton.OK, MessageBoxImage.Question);
    }

    private void Nud_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
          PerfectFit.Visibility = XCountNud.Value is not null && YCountNud.Value is not null && Image is not null
            && Image.Width % XCountNud.Value == 0 && Image.Height % YCountNud.Value == 0
            ? Visibility.Visible
            : Visibility.Hidden;
        
        Canvas.InvalidateVisual();
    }
}