using System.Collections.Specialized;
using System.Media;
using System.Windows;
using System.Windows.Input;
using SekaiLayer.Types.Collections;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SekaiLayer.UI.Controls;

public partial class TileSetConfigurationControl
{
    public event EventHandler ApplyOk = delegate { }; 
    public event EventHandler ApplyCancel = delegate { };
    public int XCount
    {
        get => XCountNud.Value ?? -1;
        set => XCountNud.Value = value;
    }
    public int YCount
    {
        get => YCountNud.Value ?? -1;
        set => YCountNud.Value = value;
    }

    public ObservableSet<Point> ExcludedTiles { get; } = [];

    public bool CanBeCancelled
    {
        get => Cancel.Visibility == Visibility.Visible;
        set => Cancel.Visibility = value ? Visibility.Visible : Visibility.Hidden;
    }
    
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
        nameof(Image),
        typeof(SKImage),
        typeof(TileSetConfigurationControl),
        new PropertyMetadata(null)
        );

    public SKImage? Image
    {
        get => (SKImage)GetValue(ImageProperty);
        set
        {
            SetValue(ImageProperty, value);
            Nud_OnValueChanged(this, new RoutedPropertyChangedEventArgs<object>(null!, null!));
        }
    }
    
    private Rect _currentSelection;
    private Point _origin;
    private bool _makingSelection;
    private Size _drawSize = Size.Empty;

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

    private static readonly SKPaint _exclusionPaint = new()
    {
        Color = new SKColor(255, 0, 0, 64),
        Style = SKPaintStyle.Fill
    };

    public TileSetConfigurationControl() : this(1, 1, [])
    {
    }
    
    public TileSetConfigurationControl(int xCount, int yCount, List<Point> exclusions)
    {
        InitializeComponent();
    
        // TODO remove when not needed
        ExcludedTiles.CollectionChanged += ExcludedTilesOnCollectionChanged;
        
        XCountNud.Value = xCount;
        YCountNud.Value = yCount;
        
        foreach (var exclusion in exclusions)
        {
            ExcludedTiles.Add(exclusion);
        }
    }

    private void ExcludedTilesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Nud_OnValueChanged(sender!, new RoutedPropertyChangedEventArgs<object>(e.OldItems!, e.NewItems!));
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

        foreach (var tile in ExcludedTiles)
        {
            canvas.DrawRect(new(
                (float)(tile.X * xDifference),
                (float)((tile.Y + 1) * yDifference),
                (float)((tile.X + 1) * xDifference),
                (float)(tile.Y * yDifference)
                ) , _exclusionPaint);
        }
        
        canvas.DrawRect(rect, _rectPaint);

        double xDrawRatio = Canvas.ActualWidth / e.Info.Rect.Width; 
        double yDrawRatio = Canvas.ActualHeight / e.Info.Rect.Height; 
        _drawSize = new(xDifference * xDrawRatio, yDifference * yDrawRatio);
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
            
            ExcludedTiles.Add(point.Value);
            return true;
        }
        
        (Point, Point)? range = GetExclusionRange(text);
        
        if (range is null)
        {
            return false;
        }

        for (int x = 0; range.Value.Item1.X <= range.Value.Item2.X; ++x)
        {
            for (int y = 0; range.Value.Item1.Y <= range.Value.Item2.Y; ++y)
            {
                ExcludedTiles.Add(new(x, y));
            }
        }

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
        
        if (numbers.Length != 2
            || !int.TryParse(numbers[0], out var x) || !int.TryParse(numbers[1], out var y)
            || x < 0 || x >= XCountNud.Value || y < 0 || y >= YCountNud.Value
            ) {
            return null;
        }
        
        return new Point(x, y);
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
        ExcludedTiles.ExceptWith(ExclusionListDisplay.SelectedItems.AsQueryable().Cast<Point>());
    }

    private void ClearExclusion_OnClick(object sender, RoutedEventArgs e)
    {
        ExcludedTiles.Clear();
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

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        ApplyOk(this, e);
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        ApplyCancel(this, e);
    }

    private void Canvas_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Manual.IsChecked != true)
        {
            return;
        }
        
        _origin = e.GetPosition(Canvas);
        _currentSelection = new Rect(_origin, _origin);
        
        _makingSelection = true;
        Canvas.CaptureMouse();
    }

    private void Canvas_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_makingSelection)
        {
            return;
        }
        
        var xStart = (int)(_currentSelection.X / _drawSize.Width);
        var yStart = (int)(_currentSelection.Y / _drawSize.Height);
        var xEnd = (int)((_currentSelection.Width + _currentSelection.X) / _drawSize.Width);
        var yEnd = (int)((_currentSelection.Height + _currentSelection.Y) / _drawSize.Height);

        if (xEnd > XCount)
        {
            xEnd = XCount;
        } 
        if (yEnd > YCount)
        {
            yEnd = YCount;
        }

        bool allContained = true;
        List<Point> selectedPoints = [];
        for (int x = xStart; x <= xEnd; ++x)
        {
            for (int y = yStart; y <= yEnd; ++y)
            {
                var p = new Point(x, y);

                if (!ExcludedTiles.Contains(p))
                {
                    allContained = false;
                }
                
                selectedPoints.Add(p);
            }
        }

        if (allContained)
        {
            ExcludedTiles.ExceptWith(selectedPoints);
        }
        else
        {
            ExcludedTiles.UnionWith(selectedPoints);   
        }
        
        _makingSelection = false;
        _currentSelection = new Rect();
        Canvas.ReleaseMouseCapture();
    }

    private void Canvas_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_makingSelection)
        {
            return;
        }

        Point p = e.GetPosition(Canvas);

        if (p.X < 0) p.X = 0; 
        if (p.Y < 0) p.Y = 0; 
        if (p.X > Canvas.Width) p.X = Canvas.Width; 
        if (p.Y > Canvas.Height) p.X = Canvas.Height;

        _currentSelection = new Rect(_origin, p);
    }
}