using UnityEngine;

public class InteractableWeaponCrate : MonoBehaviour, IInteractable
{
    [SerializeField] private int priority = 0;
    [SerializeField] private Transform uiAnchor;

    [SerializeField] private WeaponTypeSO weaponTypeSO;
    private bool isTaken = false;

    public int Priority => priority;

    public bool CanInteract(Interactor interactor)
    {
        if (isTaken) return false;
        if (interactor.Character == null) return false;
        if (interactor.Character.HasWeapon()) return false;

        return true;
    }

    public string GetInteractPrompt(Interactor interactor)
    {
        return $"F - Take ({weaponTypeSO.name})";
    }

    public bool Interact(Interactor interactor)
    {
        bool success = interactor.Character.TrySetWeapon(weaponTypeSO);

        if (success)
        {
            isTaken = true;
        }

        return success;
    }
    public Transform GetUIAnchor()
    {
        return uiAnchor != null ? uiAnchor : transform;
    }
}
