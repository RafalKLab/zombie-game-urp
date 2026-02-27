using UnityEngine;

public class InteractableActionPickUpItem : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private ItemDefinitionSO itemDefinitionSO;
    [SerializeField] private int amount = 1;
    [SerializeField] private string promt = "Pick up";
    [SerializeField] private bool destorySelfOnDepleted = false;

    private bool isDepleted = false;

    public int Priority => priority;

    public bool IsDepleted => isDepleted;

    public bool CanExecute(Interactor interactor)
    {
        if (interactor == null) return false;
        if (itemDefinitionSO == null) return false;
        if (amount <= 0) return false;

        var inventory = interactor.Inventory;
        if (inventory == null) return false;

        return inventory.GetAddableAmount(itemDefinitionSO) > 0;
    }

    public bool Execute(Interactor interactor)
    {
        if (interactor == null) return false;
        if (itemDefinitionSO == null) return false;
        if (amount <= 0) return false;

        var inventory = interactor.Inventory;
        if (inventory == null) return false;

        bool isWeapon = itemDefinitionSO is WeaponItemSO;
        int tryAmount = isWeapon ? 1 : amount;
        int remaining = inventory.TryAddReturnRemaining(itemDefinitionSO, tryAmount);
        int added = tryAmount - remaining;

        if (added <= 0)
            return false;

        // aktualizacja amount
        if (isWeapon)
            amount -= 1;
        else
            amount = remaining;

        if (amount <= 0)
        {
            isDepleted = true;

            if (destorySelfOnDepleted) Destroy(this);
        }
            

        return true;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        if (itemDefinitionSO == null) return string.Empty;

        if (amount > 1)
            return $"{promt} {itemDefinitionSO.displayName} ({amount})";

        return $"{promt} {itemDefinitionSO.displayName}";
    }
}