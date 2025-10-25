using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SekaiLayer.Types.Data;

namespace SekaiLayer.UI.Controls;

public class RemoveVaultEventArgs(VaultEntry entry) : RoutedEventArgs
{
    public VaultEntry Entry => entry;
}

public delegate void RemoveVaultEventHandler(object sender, RemoveVaultEventArgs e);

public partial class VaultDisplay
{
    public double BorderWidth { get; init; }
    public double BorderRadius { get; init; }
    public event RemoveVaultEventHandler RemoveVaultEvent = delegate { };
    private readonly VaultEntry _entry;
    public string VaultName => _entry.Name;
    
    public VaultDisplay(VaultEntry entry)
    {
        _entry = entry;
        
        InitializeComponent();
        
        NameTextBlock.Text = entry.Name;
        PathLabel.Content = entry.Path;
        RemoveButton.Visibility = Visibility.Hidden;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        Pen pen = new(Brushes.Black, BorderWidth);
        Rect bounds = new(new Point(0, 0), new Size(Width, Height));
        
        drawingContext.DrawRoundedRectangle(Brushes.White, pen, bounds, BorderRadius, BorderRadius);
        
        base.OnRender(drawingContext);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        
        Canvas.Width = sizeInfo.NewSize.Width - BorderWidth;
        Canvas.Margin = new Thickness(
            BorderWidth / 2, Canvas.Margin.Top, Canvas.Margin.Right, Canvas.Margin.Bottom
            );
        
        NameTextBlock.Width = Width - RemoveButton.Width
                        - RemoveButton.Margin.Right - RemoveButton.Margin.Left
                        - NameTextBlock.Margin.Right - NameTextBlock.Margin.Left;
    }

    private void VaultDisplay_OnLoaded(object sender, RoutedEventArgs e)
    {
        DoubleAnimation doubleAnimation = new()
        {
            From = Canvas.ActualWidth,
            To = -PathLabel.ActualWidth,
            RepeatBehavior = RepeatBehavior.Forever,
            Duration = new Duration(TimeSpan.Parse("0:0:10"))
        };
        PathLabel.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
    }

    private void VaultDisplay_OnMouseEnter(object sender, MouseEventArgs e)
    {
        RemoveButton.Visibility = Visibility.Visible;
    }

    private void VaultDisplay_OnMouseLeave(object sender, MouseEventArgs e)
    {
        RemoveButton.Visibility = Visibility.Hidden;
    }

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        RemoveVaultEvent(this, new RemoveVaultEventArgs(_entry));
    }
}