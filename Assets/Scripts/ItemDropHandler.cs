using UnityEngine;

public class ItemDropHandler : MonoBehaviour
{
    [SerializeField] private DroppedItemStack pickableInteractableItemStackPrefab;

    public static ItemDropHandler Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void DropItem(ItemStack stack, CharacterCore characterCore)
    {
        if (stack == null) return;
        if (characterCore == null) return;
        if (characterCore.inventory == null) return;
        if (pickableInteractableItemStackPrefab == null) return;

        bool success = characterCore.inventory.RemoveStack(stack);
        if (!success) return;

        Transform characterTransform = characterCore.transform;

        Vector3 spawnPosition =
            characterTransform.position +
            characterTransform.forward * 0.5f +
            Vector3.up * 0.01f;

        Quaternion spawnRotation = Quaternion.Euler(0f, characterTransform.eulerAngles.y, 0f);

        DroppedItemStack worldItem =
            Instantiate(pickableInteractableItemStackPrefab, spawnPosition, spawnRotation);

        worldItem.Init(stack);
    }
}
