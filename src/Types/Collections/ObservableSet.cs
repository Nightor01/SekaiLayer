using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SekaiLayer.Types.Collections;

public class ObservableSet<T> : ISet<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly HashSet<T> _set = [];
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection<T>.Add(T item)
    {
        _set.Add(item);

        OnCollectionChanged(NotifyCollectionChangedAction.Add, [item], -1);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        int beforeCount = _set.Count;
        _set.ExceptWith(other);

        if (_set.Count == beforeCount)
        {
            return;
        }
        
        OnCollectionChanged(NotifyCollectionChangedAction.Reset, null!, -1);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        int beforeCount = _set.Count;
        _set.IntersectWith(other);

        if (beforeCount == _set.Count)
        {
            return;
        }
        
        OnCollectionChanged(NotifyCollectionChangedAction.Reset, null!, -1);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        var otherSet = other.ToHashSet();
        List<T> added = [];

        var changed = false;
        _set.RemoveWhere(x =>
        {
            bool result = otherSet.Contains(x);
            if (result)
            {
                otherSet.ExceptWith([x]);
                changed = true;
            }

            return result;
        });

        foreach (var e in otherSet)
        {
            if (_set.Add(e))
            {
                added.Add(e);
                changed = true;
            }
        }

        if (!changed)
        {
            return;
        }
        
        OnCollectionChanged(NotifyCollectionChangedAction.Reset, null!, -1);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, added, -1);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    public void UnionWith(IEnumerable<T> other)
    {
        var otherSet = other is ISet<T> ? other : other.ToHashSet();
        List<T> added = [];

        var changed = false;
        foreach (var e in otherSet)
        {
            bool result = !_set.Contains(e);

            if (result)
            {
                _set.Add(e);
                added.Add(e);
                changed = true;
            }
        }
        
        if (!changed)
        {
            return;
        }
        
        OnCollectionChanged(NotifyCollectionChangedAction.Add, added, -1);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    public bool Add(T item)
    {
        bool result = _set.Add(item);

        if (!result)
        {
            return false;
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Add, [item], -1);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        return true;
    }

    public void Clear()
    {
        if (!_set.Any())
        {
            return;
        }
        
        _set.Clear();
        
        OnCollectionChanged(NotifyCollectionChangedAction.Reset, null!, -1);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    public bool Contains(T item) => _set.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        bool result = _set.Remove(item);
        
        if (!result)
        {
            return false;
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Reset, null!, -1);
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
        return true;
    }

    public int Count => _set.Count;
    public bool IsReadOnly => false;

    private void OnCollectionChanged(NotifyCollectionChangedAction action, List<T> changedItems, int index)
    {
        if (CollectionChanged is null)
        {
            return;
        }
        
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(
            action,
            changedItems,
            index
            ));
    }

    private void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (PropertyChanged is null)
        {
            return;
        }
        
        PropertyChanged(this, e);
    }
}