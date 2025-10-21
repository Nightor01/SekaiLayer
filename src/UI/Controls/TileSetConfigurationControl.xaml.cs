using System.Media;
using System.Windows;
using System.Windows.Input;
using SekaiLayer.Types.Collections;

namespace SekaiLayer.UI.Controls;

public partial class TileSetConfigurationControl
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
}