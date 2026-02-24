using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item Actions/Drop")]
public class DropItemActionSO : ItemActionSO
{
    public override void Execute(ItemStack stack, CharacterCore characterCore)
    {
        if (!CanExecute(stack)) return;
        Debug.Log($"[Action:Drop] {stack.definition.displayName} x{stack.amount}");
    }
}