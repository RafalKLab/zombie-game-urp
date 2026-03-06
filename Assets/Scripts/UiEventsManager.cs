using System;
using UnityEngine;

public class UiEventsManager : MonoBehaviour
{
    public static UiEventsManager Instance;

    public event EventHandler<OnOpenStorageRequestedEventArgs> OnOpenStorageRequested;
    public class OnOpenStorageRequestedEventArgs : EventArgs
    {
        public Inventory inventory;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void RequestOpenStorage(Inventory inventory)
    {
        if (inventory == null) return;

        OnOpenStorageRequested?.Invoke(this, new OnOpenStorageRequestedEventArgs { inventory = inventory });
    }
}
