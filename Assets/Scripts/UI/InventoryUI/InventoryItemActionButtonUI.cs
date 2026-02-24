using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemActionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;

    public void Init(ItemActionSO itemAction, ItemStack itemStack, CharacterCore characterCore)
    {
        buttonText.text = itemAction.actionName;

        Button button = gameObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => itemAction.Execute(itemStack, characterCore));
        }

    }
}
