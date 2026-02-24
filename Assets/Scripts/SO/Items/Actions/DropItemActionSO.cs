using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item Actions/Drop")]
public class DropItemActionSO : ItemActionSO
{
    public override void Execute(ItemStack stack, CharacterCore characterCore)
    {
        if (stack == null) return;
        if (characterCore == null) return;

        ItemDropHandler.Instance.DropItem(stack, characterCore);
    }
}