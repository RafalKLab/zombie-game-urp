using System;
using UnityEngine;

public class UiEventsManager : MonoBehaviour
{
    [SerializeField] private ResourcesUI resourcesUI;
    [SerializeField] private DefenseBaseUI defenseBaseUI;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private SelectedCharacterUI selectedCharacterUI;
    [SerializeField] private AssignCharacterMenu assignCharacterMenu;
    [SerializeField] private SelectCharacterStageUI selectCharacterStageUI;

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

    public void HideGameplayUi()
    {
        resourcesUI.gameObject.SetActive(false);
        defenseBaseUI.gameObject.SetActive(false);

        inventoryUI.HideAll();
        selectedCharacterUI.Deactivate();
        assignCharacterMenu.Hide();
    }

    public void ShowGameplayUi()
    {
        resourcesUI.gameObject.SetActive(true);
        defenseBaseUI.gameObject.SetActive(true);

        PlayableCharacter activePlayableCharacter = ActiveCharacterManager.Instance.GetActivePlayableCharacter();
        if (activePlayableCharacter != null) selectedCharacterUI.Activate(activePlayableCharacter);
    }

    public void HideSelectCharacterStageUI()
    {
        selectCharacterStageUI.gameObject.SetActive(false);
    }

    public void ShowSelectCharacterStageUI()
    {
        selectCharacterStageUI.gameObject.SetActive(true);
    }
}
