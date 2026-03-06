using UnityEngine;
using UnityEngine.UI;

public class InventoryItemTransferButton : MonoBehaviour
{
    private Button button;

    private ItemStack stack;
    private Inventory fromInventory;
    private Inventory toInventory;

    public void Init(
    ItemStack stack,
    Inventory fromInventory,
    Inventory toInventory)
    {
        this.stack = stack;
        this.fromInventory = fromInventory;
        this.toInventory = toInventory;

        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"{name}: InventoryItemTransferButton requires a Button component.", this);
            return;
        }
            
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        if (stack == null)
        {
            Debug.LogWarning("[InventoryTransfer] Stack is NULL.");
            return;
        }

        if (stack.definition == null)
        {
            Debug.LogWarning("[InventoryTransfer] Stack definition is NULL.");
            return;
        }

        if (fromInventory == null)
        {
            Debug.LogWarning("[InventoryTransfer] FromInventory is NULL.");
            return;
        }

        if (toInventory == null)
        {
            Debug.LogWarning("[InventoryTransfer] ToInventory is NULL.");
            return;
        }

        bool success = toInventory.InsertStack(stack);

        if (!success)
        {
            return;
        }

        fromInventory.RemoveStack(stack);
    }
}