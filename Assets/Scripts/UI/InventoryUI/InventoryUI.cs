using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Player inventory")]
    [SerializeField] private Transform inventoryBlock;

    [Header("Player equipped")]
    [SerializeField] private EquippedInventoryUI equippedInventoryUI;

    [Header("Player huge slots")]
    [SerializeField] private Transform hugeSlotContainer;
    [SerializeField] private HugeSlotInventoryUI hugeSlotPrefab;

    [Header("Player normal slots")]
    [SerializeField] private Transform normalSlotContainer;
    [SerializeField] private NormalSlotInventoryUI normalSlotPrefab;

    private PlayableCharacter playableCharacter;
    private CharacterCore characterCore;
    private Inventory characterInventory;

    private List<HugeSlotInventoryUI> uiHugeSlotList = new();
    private List<NormalSlotInventoryUI> uiNormalSlotList = new();

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

        inventoryBlock.gameObject.SetActive(false);
    }

    private void Start()
    {
        GameInput.Instance.OnInventoryOpen += GameInput_OnInventoryOpen;
        GameInput.Instance.OnInventoryClose += GameInput_OnInventoryClose;
    }

    private void GameInput_OnInventoryOpen()
    {
        playableCharacter = ActiveCharacterManager.Instance.GetActivePlayableCharacter();
        if (playableCharacter == null) return;

        characterCore = playableCharacter.GetCharacterCore();
        if (characterCore == null) return;

        characterInventory = playableCharacter.GetComponent<Inventory>();
        if (characterInventory == null) return;

        characterInventory.OnChanged -= CharacterInventory_OnChanged;
        characterInventory.OnChanged += CharacterInventory_OnChanged;

        // subscribe death
        characterCore.OnKilled -= CharacterCore_OnKilled;
        characterCore.OnKilled += CharacterCore_OnKilled;

        Show();
    }

    private void GameInput_OnInventoryClose()
    {
        Hide();

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
            Hide();
            return;
        }

        Show(); // rebuild
    }

    private void CharacterCore_OnKilled(object sender, System.EventArgs e)
    {
        GameInput_OnInventoryClose();
    }

    public void Show()
    {
        equippedInventoryUI.Init(characterCore.GetWeaponTypeSO(), characterCore.GetAmmoInfo());

        UpdateSlots(
            characterInventory.GetHugeSlots(),
            uiHugeSlotList,
            () => Instantiate(hugeSlotPrefab, hugeSlotContainer),
            (ui, stack) => ui.Init(stack, characterCore)
        );

        UpdateSlots(
            characterInventory.GetNormalSlots(),
            uiNormalSlotList,
            () => Instantiate(normalSlotPrefab, normalSlotContainer),
            (ui, stack) => ui.Init(stack, characterCore)
        );

        inventoryBlock.gameObject.SetActive(true);
    }

    public void Hide()
    {
        inventoryBlock.gameObject.SetActive(false);
    }

    private void UpdateSlots<TUI>(
    IReadOnlyList<ItemStack> inventorySlots,
    List<TUI> uiSlots,
    System.Func<TUI> createUI,
    System.Action<TUI, ItemStack> bindData
    ) where TUI : MonoBehaviour
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                ItemStack itemStack = inventorySlots[i];

                if (i >= uiSlots.Count || uiSlots[i] == null)
                {
                    TUI uiBlock = createUI();
                    uiSlots.Add(uiBlock);
                }

                bindData(uiSlots[i], itemStack);
                uiSlots[i].gameObject.SetActive(true);
            }

            for (int i = inventorySlots.Count; i < uiSlots.Count; i++)
            {
                if (uiSlots[i] != null)
                    uiSlots[i].gameObject.SetActive(false);
            }
        }
}
