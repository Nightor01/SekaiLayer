using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SekaiLayer.UI.Controls;

public class PreviewTextBox : TextBox
{
    private static readonly DependencyPropertyKey _hasTextPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasText), 
        typeof(bool),
        typeof(TextBox),
        new PropertyMetadata()
        );

    public static readonly DependencyProperty HasTextProperty = _hasTextPropertyKey.DependencyProperty;

    public static readonly DependencyProperty TextPreviewProperty = DependencyProperty.Register(
        nameof(TextPreview),
        typeof(string),
        typeof(PreviewTextBox)
        );
    
    public static readonly DependencyProperty PreviewColorProperty = DependencyProperty.Register(
        nameof(PreviewColor),
        typeof(Brush),
        typeof(PreviewTextBox),
        new PropertyMetadata(Brushes.Gray)
        );

    public bool HasText
    {
        get => (bool)GetValue(HasTextProperty);
        private set => SetValue(_hasTextPropertyKey, value);
    }

    public string TextPreview
    {
        get => (string)GetValue(TextPreviewProperty);
        set => SetValue(TextPreviewProperty, value);
    }

    public Brush PreviewColor
    {
        get => (Brush)GetValue(PreviewColorProperty);
        set => SetValue(PreviewColorProperty, value);
    }

    static PreviewTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PreviewTextBox),
            new FrameworkPropertyMetadata(typeof(PreviewTextBox))
            );
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        HasText = Text.Length > 0;
    }
}