namespace SekaiLayer.Events;

public class CancellableEventArgs : EventArgs
{
    public bool Canceled { get; private set; } = false;

    public void Cancel() => Canceled = true;
}

public delegate void CancellableEventHandler(object sender, CancellableEventArgs args);