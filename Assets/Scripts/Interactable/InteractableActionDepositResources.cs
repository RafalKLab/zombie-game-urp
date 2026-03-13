using UnityEngine;

public class InteractableActionDepositResources : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private string executePrompt = "Deposit resources";
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private Faction faction;
    public bool IsDepleted => false;

    public int Priority => priority;

    private void Start()
    {
        if (FactionBaseRegistry.Instance == null)
        {
            Debug.LogError("[InteractableActionDepositResources] FactionBaseRegistry.Instance is null", this);
            return;
        }

        BaseManager baseManager = FactionBaseRegistry.Instance.GetBaseManagerByFaction(faction);

        if (baseManager == null)
        {
            Debug.LogError($"[InteractableActionDepositResources] BaseManager not found for faction: {faction}", this);
            return;
        }

        resourceManager = baseManager.GetResourceManager();

        if (resourceManager == null)
        {
            Debug.LogError($"[InteractableActionDepositResources] ResourceManager missing for faction: {faction}", this);
            return;
        }
    }

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