using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Items/Weapon Item")]
public class WeaponItemSO : ItemDefinitionSO
{
    [Header("Pick ONE")]
    public WeaponTypeSO weaponTypeSO;
    public MeleeWeaponTypeSO meleeWeaponTypeSO;
    public bool useMelee;
}