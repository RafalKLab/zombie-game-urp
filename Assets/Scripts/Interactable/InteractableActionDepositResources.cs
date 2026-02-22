using UnityEngine;

public class InteractableActionDepositResources : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private string executePrompt = "Deposit resources";
    [SerializeField] private ResourceManager resourceManager;
    public bool IsDepleted => false;

    public int Priority => priority;

    public bool CanExecute(Interactor interactor)
    {
        if (resourceManager == null) return false;
        if (interactor == null) return false;

        var inventory = interactor.Inventory;
        if (inventory == null) return false;

        return HasAnyResource(inventory);
    }

    public bool Execute(Interactor interactor)
    {
        if (resourceManager == null) return false;
        if (interactor == null) return false;

        var inventory = interactor.Inventory;
        if (inventory == null) return false;

        bool depositedSomething = false;

        depositedSomething |= DepositFromList(inventory, inventory.GetNormalSlots());
        depositedSomething |= DepositFromList(inventory, inventory.GetHugeSlots());

        return depositedSomething;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        return executePrompt;
    }

    // ------------------------

    private bool HasAnyResource(Inventory inventory)
    {
        foreach (var stack in inventory.GetNormalSlots())
        {
            if (stack == null) continue;

            if (stack.definition is ResourceItemSO)
                return true;
        }

        foreach (var stack in inventory.GetHugeSlots())
        {
            if (stack == null) continue;

            if (stack.definition is ResourceItemSO)
                return true;
        }

        return false;
    }

    private bool DepositFromList(Inventory inventory, System.Collections.Generic.IReadOnlyList<ItemStack> list)
    {
        bool didSomething = false;

        for (int i = 0; i < list.Count; i++)
        {
            var stack = list[i];
            if (stack == null) continue;

            if (stack.definition is ResourceItemSO resourceItem)
            {
                int amount = stack.amount;

                resourceManager.AddResourceAmount(
                    resourceItem.resourceType,
                    amount * resourceItem.resourceUnits
                );

                inventory.TryRemove(stack.definition, amount);
                didSomething = true;
            }
        }

        return didSomething;
    }
}