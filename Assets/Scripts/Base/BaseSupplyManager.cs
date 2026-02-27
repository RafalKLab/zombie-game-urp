using UnityEngine;

public class BaseSupplyManager : MonoBehaviour
{
    /// <summary>
    /// Very simple: grants ammo into character inventory based on currently equipped weapon.
    /// Does NOT spend resources (yet).
    /// </summary>
    public bool TrySupplyAmmoFor(CharacterCore characterCore)
    {
        if (characterCore == null) return false;
        if (!characterCore.HasWeapon()) return false;

        Inventory inventory = characterCore.inventory;
        if (inventory == null) return false;

        WeaponTypeSO weaponTypeSO = characterCore.GetWeaponTypeSO();
        if (weaponTypeSO == null) return false;

        if (weaponTypeSO.requiredAmmoItemSO == null) return false;
        if (weaponTypeSO.magazineCapacity <= 0) return false;

        int amountToGrant = weaponTypeSO.magazineCapacity;

        if (amountToGrant <= 0) return false;

        int remaining = inventory.TryAddReturnRemaining(weaponTypeSO.requiredAmmoItemSO, amountToGrant);
        int added = amountToGrant - remaining;

        return added > 0;
    }
}