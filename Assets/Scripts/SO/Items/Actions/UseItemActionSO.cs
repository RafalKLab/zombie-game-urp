using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Item Actions/Use")]
public class UseItemActionSO : ItemActionSO
{
    public override void Execute(ItemStack stack)
    {
        if (!CanExecute(stack)) return;
        Debug.Log($"[Action:Use] {stack.definition.displayName}");
    }
}