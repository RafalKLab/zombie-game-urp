using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Items/Resource Item")]
public class ResourceItemSO : ItemDefinitionSO
{
    [Header("Converts To Resource Units")]
    public ResourceTypeSO resourceType;
    public int resourceUnits = 1;
}