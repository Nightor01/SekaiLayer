using System.Media;
using System.Windows;
using System.Windows.Controls;
using SekaiLayer.Types.Collections;

namespace SekaiLayer.UI.Controls;

public partial class TileSetConfigurationControl : UserControl
{
    // TODO make as observable set
    public ObservableSet<Rect> ExcludedPoints { get; } = [];
    
    public TileSetConfigurationControl()
    {
        InitializeComponent();
    }

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
}