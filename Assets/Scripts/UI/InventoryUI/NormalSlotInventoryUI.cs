using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NormalSlotInventoryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemAmount;

    [Header("Item actions")]
    [SerializeField] private Transform actionMenu;
    [SerializeField] private InventoryItemActionButtonUI actionButtonPrefab;

    private ItemStack itemStack;

    private readonly List<InventoryItemActionButtonUI> actionButtonPool = new();

    public void Init(ItemStack itemStack, CharacterCore characterCore)
    {
        if (itemImage == null) return;
        if (itemAmount == null) return;

        this.itemStack = itemStack;

        // ukryj menu przy re-inicie
        if (actionMenu != null)
            actionMenu.gameObject.SetActive(false);

        // schowaj wszystkie buttony z puli
        for (int i = 0; i < actionButtonPool.Count; i++)
        {
            if (actionButtonPool[i] != null)
                actionButtonPool[i].gameObject.SetActive(false);
        }

        if (itemStack == null)
        {
            itemImage.gameObject.SetActive(false);
            itemAmount.gameObject.SetActive(false);
            return;
        }

        itemImage.gameObject.SetActive(true);
        itemAmount.gameObject.SetActive(true);
        itemImage.sprite = itemStack.definition.icon;
        itemAmount.text = itemStack.amount.ToString();

        int index = 0;
        foreach (var itemAction in itemStack.definition.actions)
        {
            InventoryItemActionButtonUI btn;

            if (index < actionButtonPool.Count && actionButtonPool[index] != null)
            {
                btn = actionButtonPool[index];
            }
            else
            {
                btn = Instantiate(actionButtonPrefab, actionMenu);
                actionButtonPool.Add(btn);
            }

            btn.Init(itemAction, itemStack, characterCore);
            btn.gameObject.SetActive(true);

            index++;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemStack == null) return;

        if (actionMenu != null)
            actionMenu.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (actionMenu != null)
            actionMenu.gameObject.SetActive(false);
    }
}