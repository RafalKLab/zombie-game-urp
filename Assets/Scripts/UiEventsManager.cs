using System;
using UnityEngine;

public class UiEventsManager : MonoBehaviour
{
    [SerializeField] MainOverviewUI mainOverviewUI;

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

    private void Start()
    {
        GameInput.Instance.OnToggleSelectCharacterStage += GameInput_OnToggleSelectCharacterStage;
        GameInput.Instance.OnToggleBaseViewUI += GameInput_OnToggleBaseViewUI;
    }

    private void GameInput_OnToggleBaseViewUI()
    {
        mainOverviewUI.ToggleUiOverview(UiOverviewType.BaseView);
    }

    public void GameInput_OnToggleSelectCharacterStage()
    {
        mainOverviewUI.ToggleUiOverview(UiOverviewType.Characters);
    }

    public void RequestOpenStorage(Inventory inventory)
    {
        if (inventory == null) return;

        OnOpenStorageRequested?.Invoke(this, new OnOpenStorageRequestedEventArgs { inventory = inventory });
    }
}
