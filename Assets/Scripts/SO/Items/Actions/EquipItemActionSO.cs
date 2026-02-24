using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Item Actions/Equip")]
public class EquipItemActionSO : ItemActionSO
{
    public override void Execute(ItemStack stack, CharacterCore characterCore)
    {
        if (stack == null) return;
        if (stack.definition == null) return;
        if (characterCore == null) return;

        characterCore.TryToEquipItem(stack);
    }
}