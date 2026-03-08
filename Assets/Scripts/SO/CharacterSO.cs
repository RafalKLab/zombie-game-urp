using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Visual")]
    public Transform prefab;
    public Transform mesh;
    public Sprite sprite;

    [Header("Identity")]
    public string characterName;

    [Header("Movement")]
    public float walkSpeed;
    public float runSpeed;

    [Header("Stats")]
    public float maxHealth;

    [Header("Shooting")]
    public float aimingTime;

    [Header("AI Settings")]
    public float enemyDetectRadius = 50;

}
