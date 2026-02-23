using UnityEngine;

public enum ItemCategory
{
    Resource,
    Weapon,
    WeaponAmmo,
    Trade,
    Misc
}

public class ItemDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    [TextArea] public string description;

    [Header("Visual")]
    public Sprite icon;

    [Header("Stacking")]
    public bool stackable = true;
    public int maxStack = 10;

    [Header("Category")]
    public ItemCategory category;

    [Header("Inventory")]
    public InventorySlotType requiredSlot = InventorySlotType.Normal;
}