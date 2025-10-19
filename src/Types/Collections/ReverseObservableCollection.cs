using System.Collections.ObjectModel;

namespace SekaiLayer.Types.Collections;

public class ReverseObservableCollection<T> : ObservableCollection<T>
{
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(Count - index, item);
    }
}