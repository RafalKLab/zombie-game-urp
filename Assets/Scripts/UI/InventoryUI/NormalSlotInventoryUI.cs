using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NormalSlotInventoryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image hoverImage;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemAmount;

    private ItemStack itemStack;

    public void Init(ItemStack itemStack)
    {
        if (itemImage == null) return;
        if (itemAmount == null) return;
        this.itemStack = itemStack;

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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemStack == null)
            return;

        if (hoverImage != null)
            hoverImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.gameObject.SetActive(false);
    }
}
