using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;

public class InteractableActionPickUpStack : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private string promt = "Pick up";

    [SerializeField]  private ItemStack itemStack;
    private int amount = 1;

    private bool isDepleted = false;

    public int Priority => priority;

    public bool IsDepleted => isDepleted;

    public bool CanExecute(Interactor interactor)
    {
        if (amount <= 0) return false;
        if (isDepleted) return false;
        if (itemStack == null) return false;
        if (interactor == null) return false;
        if (interactor.Inventory == null) return false;


        return interactor.Inventory.HasEmptySlotFor(itemStack);
    }

    public bool Execute(Interactor interactor)
    {
        if (!CanExecute(interactor))
            return false;

        bool success = TryApply(interactor);

        if (!success)
            return false;

        amount--;
        isDepleted = true;
        return true;
    }

    private bool TryApply(Interactor interactor)
    {
        if (itemStack.definition is WeaponItemSO weaponItemSO &&
            !interactor.Character.HasWeapon() &&
                weaponItemSO.useMelee == false)
        {
            return interactor.Character.TrySetWeapon(
                weaponItemSO,
                itemStack.weaponRuntimeState
            );
        }

        return interactor.Inventory.InsertStack(itemStack);
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        if (itemStack == null)
            return promt;

        if (itemStack.definition is WeaponItemSO weaponItemSO)
        {
            if (weaponItemSO.useMelee)
            {
                return $"{promt} {weaponItemSO.displayName}";
            } else
            {
                int capacity = weaponItemSO.weaponTypeSO.magazineCapacity;
                int current = itemStack.weaponRuntimeState?.CurrentMagazineAmmo ?? capacity;
                return $"{promt} {weaponItemSO.displayName} ({current} / {capacity})";
            }
        }

        return $"{promt} {itemStack.definition.displayName} ({itemStack.amount})";
    }

    public void SetStack(ItemStack itemStack)
    {
        this.itemStack = itemStack;
    }
}
