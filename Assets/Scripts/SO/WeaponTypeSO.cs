using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/WeaponType")]
public class WeaponTypeSO : ScriptableObject
{
    [Header("Identity")]
    public WeaponType weaponType;

    [Header("Reload")]
    [Min(1)] public int magazineCapacity = 1;
    public float reloadTime = 1f;

    [Header("Reload audio")]
    public AudioClip reloadClip;
    [Range(0f, 1f)] public float reloadVolume = 1f;

    [Header("Combat")]
    [Min(0f)] public float damage = 10f;
    [Min(0f)] public float range = 20f;
    [Min(0f)] public float effectiveRange = 20f;

    [Range(0f, 1f)]
    public float accuracy = 0.9f;

    [Min(0f)]
    public float maxSpreadAngle = 10f;

    [Range(0f, 1f)] public float accuracyOutEffectiveRange = 0.4f;

    [Tooltip("Seconds between shots")]
    [Min(0f)] public float shootCooldown = 0.2f;

    [Header("Shot cooldown audio")]
    public AudioClip shotCooldownClip;
    [Range(0f, 1f)] public float shotCooldownVolume = 1f;

    [Tooltip("How many valid targets can be damaged by a single shot (penetration)")]
    [Min(1)] public int maxPenetrations = 1;

    [Header("Raycast")]
    [Tooltip("Layers that the shot raycast can hit (e.g., Environment + EnemyHitbox)")]
    public LayerMask layerMask;

    [Header("Visual")]
    public Transform prefab;

    [Header("Shot audio")]
    public AudioClip shotClip;
    [Range(0f, 1f)] public float shotVolume = 1f;
}

public enum WeaponType
{
    Pistol,
    Rifle,
    Melee
}
