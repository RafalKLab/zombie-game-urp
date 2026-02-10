using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/EnemyType")]
public class EnemyTypeSO : ScriptableObject
{
    public Transform prefab;
    public string enemyTypeName;
    public float maxHealth;

    [Header("Wander")]
    public float wanderSpeed;
    public float wanderRadius;
    public float idleTimeMin;
    public float idleTimeMax;

    [Header("Chase")]
    public float chaseSpeed;
    public float detectRadius;

    [Header("Attack")]
    public float attackRange;
    public float attackCooldown;
    public float attackDamage;
}
