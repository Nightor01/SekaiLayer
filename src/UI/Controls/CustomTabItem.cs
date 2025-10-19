using System.Windows.Controls;

namespace SekaiLayer.UI.Controls;

public class CustomTabItem
{
    public required string Header { get; init; }
    public required Guid Id { get; init; }
    public required Control Content { get; init; }    
}