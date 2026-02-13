using System;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAnimatorFacade : MonoBehaviour
{
    public event Action AttackHit;
    public event Action AttackEnd;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Animator Params")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string attackParam = "Attack";
    [SerializeField] private string deathParam = "Death";
    [SerializeField] private float speedDampTime = 0.15f;

    private int speedHash;
    private int attackHash;
    private int deathHash;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!agent) agent = GetComponent<NavMeshAgent>();

        speedHash = Animator.StringToHash(speedParam);
        attackHash = Animator.StringToHash(attackParam);
        deathHash = Animator.StringToHash(deathParam);
    }

    private void Update()
    {
        if (!animator || !agent) return;

        float worldSpeed = agent.velocity.magnitude;
        animator.SetFloat(speedHash, worldSpeed, speedDampTime, Time.deltaTime);
    }

    public void PlayAttack()
    {
        if (!animator) return;

        animator.SetTrigger(attackHash);
    }
    public void AnimEvent_AttackHit()
    {
        AttackHit?.Invoke();
    }

    public void AnimEvent_AttackEnd()
    {
        AttackEnd?.Invoke();
    }
    public void PlayDeath()
    {
        if (!animator) return;

        animator.SetTrigger(deathHash);
    }
}
