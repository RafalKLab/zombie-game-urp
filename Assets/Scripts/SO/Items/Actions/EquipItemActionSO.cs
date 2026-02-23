using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Item Actions/Equip")]
public class EquipItemActionSO : ItemActionSO
{
    public override void Execute(ItemStack stack)
    {
        if (!CanExecute(stack)) return;
        Debug.Log($"[Action:Equip] {stack.definition.displayName}");
    }
}