using System.Collections.Generic;
using UnityEngine;
using static CharacterWeaponHandler;

public class InventoryUI : MonoBehaviour
{
    [Header("Player inventory")]
    [SerializeField] private Transform inventoryBlock;
    [SerializeField] private Transform normalSlotContainer;
    [SerializeField] private Transform hugeSlotContainer;

    [Header("Base inventory")]
    [SerializeField] private Transform baseInventoryBlock;
    [SerializeField] private Transform baseNormalSlotContainer;
    [SerializeField] private Transform baseHugeSlotContainer;

    [Header("Player equipped")]
    [SerializeField] private EquippedInventoryUI equippedInventoryUI;
    [SerializeField] private EquippedInventoryUI equippedMeleeInventoryUI;

    [Header("Prefabs")]
    [SerializeField] private NormalSlotInventoryUI normalSlotPrefab;
    [SerializeField] private HugeSlotInventoryUI hugeSlotPrefab;
    [SerializeField] private InventoryItemTransferButton inventoryItemTransferButton;

    private PlayableCharacter playableCharacter;
    private CharacterCore characterCore;
    private Inventory characterInventory;

    private List<HugeSlotInventoryUI> uiHugeSlotList = new();
    private List<NormalSlotInventoryUI> uiNormalSlotList = new();

    private List<HugeSlotInventoryUI> uiBaseHugeSlotList = new();
    private List<NormalSlotInventoryUI> uiBaseNormalSlotList = new();

    private bool CanTransfer =>
        mainInventory != null &&
        secondaryInventory != null &&
        inventoryItemTransferButton != null;

    private Inventory mainInventory;
    private Inventory secondaryInventory;

    private void Awake()
    {
        bool ok = true;

        if (inventoryBlock == null)
        {
            Debug.LogError($"{name} InventoryUI: missing reference: inventoryBlock", this);
            ok = false;
        }

        if (equippedInventoryUI == null)
        {
            Debug.LogError($"{name} InventoryUI: missing reference: equippedInventoryUI", this);
            ok = false;
        }

        if (equippedMeleeInventoryUI == null)
        {
            Debug.LogError($"{name} InventoryUI: missing reference: equippedMeleeInventoryUI", this);
            ok = false;
        }

        if (hugeSlotContainer == null)
        {
            Debug.LogError($"{name} InventoryUI: missing reference: hugeSlotContainer", this);
            ok = false;
        }

        if (hugeSlotPrefab == null)
        {
            Debug.LogError($"{name} InventoryUI: missing reference: hugeSlotPrefab", this);
            ok = false;
        }

        if (normalSlotContainer == null)
        {
            Debug.LogError($"{name} InventoryUI: missing reference: normalSlotContainer", this);
            ok = false;
        }

        if (normalSlotPrefab == null)
        {
            Debug.LogError($"{name} InventoryUI: missing reference: normalSlotPrefab", this);
            ok = false;
        }

        if (!ok)
        {
            enabled = false;
            return;
        }

        HideAll();
    }

    private void Start()
    {
        GameInput.Instance.OnInventoryOpen += GameInput_OnInventoryOpen;
        GameInput.Instance.OnInventoryClose += GameInput_OnInventoryClose;
        UiEventsManager.Instance.OnOpenStorageRequested += Instance_OnOpenStorageRequested;
    }

    private void Instance_OnOpenStorageRequested(object sender, UiEventsManager.OnOpenStorageRequestedEventArgs e)
    {
        // we allow open stoarge alwasy with main invetory, this does
        // not suppoer openning storage where there is no active character

        if (secondaryInventory != null)
            secondaryInventory.OnChanged -= SecondaryInventory_OnChanged;
        
        secondaryInventory = e.inventory;

        if (secondaryInventory != null)
        {
            secondaryInventory.OnChanged -= SecondaryInventory_OnChanged;
            secondaryInventory.OnChanged += SecondaryInventory_OnChanged;
        }


        GameInput_OnInventoryOpen();
        ShowSecondary(e.inventory);
    }

    private void GameInput_OnInventoryOpen()
    {
        // refactor maybe
        // open only when gameplay ui active
        if (UiEventsManager.Instance.IsGameplayUi() != true) return;

        playableCharacter = ActiveCharacterManager.Instance.GetActivePlayableCharacter();
        if (playableCharacter == null) return;

        characterCore = playableCharacter.GetCharacterCore();
        if (characterCore == null) return;

        characterInventory = playableCharacter.GetComponent<Inventory>();
        if (characterInventory == null) return;

        mainInventory = characterInventory;

        characterInventory.OnChanged -= CharacterInventory_OnChanged;
        characterInventory.OnChanged += CharacterInventory_OnChanged;

        // subscribe death
        characterCore.OnKilled -= CharacterCore_OnKilled;
        characterCore.OnKilled += CharacterCore_OnKilled;

        ShowMain();
    }

    private void GameInput_OnInventoryClose()
    {
        HideAll();

        // unsubscribe first
        if (characterInventory != null)
            characterInventory.OnChanged -= CharacterInventory_OnChanged;

        if (characterCore != null)
            characterCore.OnKilled -= CharacterCore_OnKilled;

        playableCharacter = null;
        characterCore = null;
        characterInventory = null;
    }

    private void OnDisable()
    {
        GameInput.Instance.OnInventoryOpen -= GameInput_OnInventoryOpen;
        GameInput.Instance.OnInventoryClose -= GameInput_OnInventoryClose;

        // safety unsubs
        if (characterInventory != null)
            characterInventory.OnChanged -= CharacterInventory_OnChanged;

        if (characterCore != null)
            characterCore.OnKilled -= CharacterCore_OnKilled;
    }

    private void CharacterInventory_OnChanged()
    {
        if (inventoryBlock == null) return;
        if (!inventoryBlock.gameObject.activeSelf) return;

        if (characterCore == null || characterInventory == null)
        {
            HideAll();
            return;
        }

        ShowMain();
    }

    private void SecondaryInventory_OnChanged()
    {
        if (baseInventoryBlock == null) return;
        if (!baseInventoryBlock.gameObject.activeSelf) return;

        if (secondaryInventory == null || mainInventory == null)
        {
            HideAll();
            return;
        }

        ShowSecondary(secondaryInventory);
    }

    private void CharacterCore_OnKilled(object sender, System.EventArgs e)
    {
        GameInput_OnInventoryClose();
    }

    public void ShowMain()
    {
        equippedInventoryUI.Init(characterCore.GetWeaponItemSO(), characterCore.GetAmmoInfo());
        equippedMeleeInventoryUI.Init(characterCore.GetMeleeWeaponItemSO(), new AmmoInfo());

        UpdateSlots(
            characterInventory.GetHugeSlots(),
            uiHugeSlotList,
            () => Instantiate(hugeSlotPrefab, hugeSlotContainer),
            (ui, stack) => ui.Init(stack, characterCore),
            mainInventory,
            secondaryInventory,
            true
        );

        UpdateSlots(
            characterInventory.GetNormalSlots(),
            uiNormalSlotList,
            () => Instantiate(normalSlotPrefab, normalSlotContainer),
            (ui, stack) => ui.Init(stack, characterCore),
            mainInventory,
            secondaryInventory,
            true
        );

        inventoryBlock.gameObject.SetActive(true);
    }

    public void ShowSecondary(Inventory inventory)
    {
        UpdateSlots(
            inventory.GetHugeSlots(),
            uiBaseHugeSlotList,
            () => Instantiate(hugeSlotPrefab, baseHugeSlotContainer),
            (ui, stack) => ui.Init(stack, characterCore, false),
            secondaryInventory,
            mainInventory,
            false
        );

        UpdateSlots(
            inventory.GetNormalSlots(),
            uiBaseNormalSlotList,
            () => Instantiate(normalSlotPrefab, baseNormalSlotContainer),
            (ui, stack) => ui.Init(stack, characterCore, false),
            secondaryInventory,
            mainInventory,
            false
        );

        baseInventoryBlock.gameObject.SetActive(true);
    }

    // always hide main and secondary invenotry windows
    public void HideAll()
    {
        if (secondaryInventory != null)
            secondaryInventory.OnChanged -= SecondaryInventory_OnChanged;

        mainInventory = null;
        secondaryInventory = null;

        inventoryBlock.gameObject.SetActive(false);
        baseInventoryBlock.gameObject.SetActive(false);
    }

    private void UpdateSlots<TUI>(
    IReadOnlyList<ItemStack> inventorySlots,
    List<TUI> uiSlots,
    System.Func<TUI> createUI,
    System.Action<TUI, ItemStack> bindData,
    Inventory fromInventory,
    Inventory toInventory,
    bool includeEmptySlots
) where TUI : MonoBehaviour
    {
        int uiIndex = 0;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            ItemStack itemStack = inventorySlots[i];

            if (!includeEmptySlots && itemStack == null)
                continue;

            if (uiIndex >= uiSlots.Count || uiSlots[uiIndex] == null)
            {
                TUI uiBlock = createUI();
                if (uiIndex < uiSlots.Count) uiSlots[uiIndex] = uiBlock;
                else uiSlots.Add(uiBlock);
            }

            // Bind visuals
            bindData(uiSlots[uiIndex], itemStack);
            uiSlots[uiIndex].gameObject.SetActive(true);

            InventoryItemTransferButton transferBtn =
                uiSlots[uiIndex].GetComponentInChildren<InventoryItemTransferButton>(true);

            bool allowTransferHere =
                CanTransfer &&
                fromInventory != null &&
                toInventory != null;

            if (!allowTransferHere)
            {
                if (transferBtn != null)
                    transferBtn.gameObject.SetActive(false);
            }
            else
            {
                if (transferBtn == null)
                    transferBtn = Instantiate(inventoryItemTransferButton, uiSlots[uiIndex].transform);

                transferBtn.gameObject.SetActive(true);
                transferBtn.Init(itemStack, fromInventory, toInventory);
            }

            uiIndex++;
        }

        for (int i = uiIndex; i < uiSlots.Count; i++)
        {
            if (uiSlots[i] == null) continue;

            uiSlots[i].gameObject.SetActive(false);

            var transferBtn = uiSlots[i].GetComponentInChildren<InventoryItemTransferButton>(true);
            if (transferBtn != null)
                transferBtn.gameObject.SetActive(false);
        }
    }
}
