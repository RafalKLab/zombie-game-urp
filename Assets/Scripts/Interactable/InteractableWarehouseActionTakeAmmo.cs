using UnityEngine;

public class InteractableWarehouseActionTakeAmmo : MonoBehaviour, IInteractableAction
{
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private int priority = 0;
    [SerializeField] private WeaponAmmoItemSO ammoItemSO;
    [SerializeField] private string promt = "Take ammo";

    public int Priority => priority;

    public bool IsDepleted => false;

    public bool CanExecute(Interactor interactor)
    {
        if (resourceManager == null) return false;
        if (interactor == null) return false;
        if (interactor.Inventory == null) return false;

        // Quote exactly 1 currency bundle
        if (!resourceManager.TryQuoteOneCurrency(ammoItemSO, out int granted))
            return false;

        if (granted <= 0)
            return false;

        // Check if inventory can fit the granted bundle
        return interactor.Inventory.HasSpaceFor(ammoItemSO, granted);
    }

    public bool Execute(Interactor interactor)
    {
        if (resourceManager == null) return false;
        if (interactor == null) return false;
        if (interactor.Inventory == null) return false;

        // Quote first so Execute is consistent with CanExecute
        if (!resourceManager.TryQuoteOneCurrency(ammoItemSO, out int granted))
            return false;

        if (granted <= 0)
            return false;

        if (!interactor.Inventory.HasSpaceFor(ammoItemSO, granted))
            return false;

        // Commit: spend 1 currency and receive the bundle
        if (!resourceManager.TryWithdrawOneCurrency(ammoItemSO, out int withdrawn))
            return false;

        if (withdrawn <= 0)
            return false;

        interactor.Inventory.TryAddReturnRemaining(ammoItemSO, withdrawn);
        return true;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        string reason = string.Empty;
        int granted = 0;

        if (resourceManager == null)
        {
            reason = "ResourceManager is null";
        }
        else if (ammoItemSO == null)
        {
            reason = "AmmoItemSO is null";
        }
        else if (interactor == null)
        {
            reason = "Interactor is null";
        }
        else if (interactor.Inventory == null)
        {
            reason = "Inventory is null";
        }
        else if (!resourceManager.TryQuoteOneCurrency(ammoItemSO, out granted) || granted <= 0)
        {
            reason = "Not enough resources";
        }
        else if (!interactor.Inventory.HasSpaceFor(ammoItemSO, granted))
        {
            reason = "Inventory full";
        }

        if (!string.IsNullOrEmpty(reason))
            return $"{promt} {ammoItemSO?.displayName} - {reason}";

        // Cost is always 1 currency in this simplified model
        return $"{promt} {ammoItemSO.displayName} ({granted}x) - Cost: 1";
    }
}