using UnityEngine;

public class InteractableActionTakeWeapon : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private WeaponItemSO weaponItemSO;
    [SerializeField] private int amount = 1;
    [SerializeField] private string executePrompt = "Take weapon";

    private bool isDepleted = false;

    public int Priority => priority;

    public bool IsDepleted => isDepleted;

    public bool CanExecute(Interactor interactor)
    {
        if (amount <= 0) return false;
        if (interactor == null) return false;
        if (interactor.Character == null) return false;
        if (weaponItemSO == null) return false;

        if (interactor.Character.HasWeapon())
        {
            var inv = interactor.Inventory;
            if (inv == null) return false;

            return inv.GetAddableAmount(weaponItemSO) > 0;
        }

        return true;
    }

    public bool Execute(Interactor interactor)
    {
        if (!CanExecute(interactor)) return false;

        var character = interactor.Character;

        if (!character.HasWeapon())
        {
            bool equipped = character.TrySetWeapon(weaponItemSO);
            if (!equipped) return false;
        }
        else
        {
            var inv = interactor.Inventory;
            if (inv == null) return false;

            if (!inv.TryAdd(weaponItemSO, 1))
                return false;
        }

        amount--;

        if (amount <= 0)
            isDepleted = true;

        return true;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        if (weaponItemSO == null) return executePrompt;
        return $"{executePrompt}";
    }
}