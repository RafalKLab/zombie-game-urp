using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/MeleeWeaponType")]
public class MeleeWeaponTypeSO : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public WeaponType weaponType;

    [Header("Combat")]
    [Min(0f)] public float damage = 10f;
    [Min(0f)] public float range = 20f;
    [Min(0f)] public float hitCooldown = 1f;
    public LayerMask targetableMask;

    [Header("Visual")]
    public MeleeWeapon prefab;

    [Header("Hit audio")]
    public AudioClip hitFleshClip;
    [Range(0f, 1f)] public float hitFleshClipVolume = 1f;
}
