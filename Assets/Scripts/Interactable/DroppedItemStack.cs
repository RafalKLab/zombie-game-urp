using UnityEngine;

public class DroppedItemStack : MonoBehaviour
{
    [SerializeField] private InteractableActionPickUpStack interactableActionPickUpStack;

    [Header("Weapon visual")]
    [SerializeField] private Transform weaponVisualPosition;

    [Header("Default item visual")]
    [SerializeField] private Transform itemVisualPosition;
    [SerializeField] private Transform defaultItemPrefab;

    public void Init(ItemStack itemStack)
    {
        if (interactableActionPickUpStack == null) return;

        interactableActionPickUpStack.SetStack(itemStack);

        if (itemStack.definition is WeaponItemSO weaponItemSO)
        {
            if (weaponItemSO.useMelee)
                Instantiate(weaponItemSO.meleeWeaponTypeSO.prefab, weaponVisualPosition);
            else
                Instantiate(weaponItemSO.weaponTypeSO.prefab, weaponVisualPosition);
        }
        else
        {
            Instantiate(defaultItemPrefab, itemVisualPosition);
        }
    }
}
