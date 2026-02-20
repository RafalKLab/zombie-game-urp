using UnityEngine;

public class InteractableActionTakeWeapon : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private WeaponTypeSO weaponTypeSO;
    [SerializeField] private int amount = 1;
    [SerializeField] private string executePrompt = "Take weapon";


    public int Priority => priority;

    public bool CanExecute(Interactor interactor)
    {
        if (amount <= 0) return false;
        if (interactor.Character == null) return false;
        if (interactor.Character.HasWeapon()) return false;

        return true;
    }

    public bool Execute(Interactor interactor)
    {
        bool success = interactor.Character.TrySetWeapon(weaponTypeSO);

        if (success)
        {
            amount--;
        }

        return success;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        return executePrompt;
    }
}
