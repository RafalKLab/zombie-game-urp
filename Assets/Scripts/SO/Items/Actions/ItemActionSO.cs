using UnityEngine;

public abstract class ItemActionSO : ScriptableObject
{
    [Header("UI")]
    public string actionName = "Action";

    public virtual bool CanExecute(ItemStack stack) => stack != null && stack.definition != null;

    public abstract void Execute(ItemStack stack);
}