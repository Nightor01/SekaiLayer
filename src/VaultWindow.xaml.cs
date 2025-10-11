using System.Windows;

namespace SekaiLayer;

public partial class VaultWindow : Window
{
    public required string WorkingPath { get; init; }
    
    public VaultWindow()
    {
        InitializeComponent();
    }
}