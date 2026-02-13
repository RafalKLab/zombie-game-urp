using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ZombieAnimatorFacade))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AiTarget))]
public class ZombieAi : MonoBehaviour
{
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] private LayerMask targetableMask;
    [SerializeField] private ZombieAnimatorFacade zombieAnimatorFacade;

    private Faction entityFaction;

    private NavMeshAgent agent;

    private State activeState;

    private float idleTimer = 0f;
    private bool isIdling = false;

    private float detectionCooldown = 1f; // raz na sekunde
    private float detectionTimer = 0;

    private float chaseDistanceCheckCooldown = 0.5f; // dwa razy na sekunde
    private float chaseDistanceCheckTimer = 0;

    private float attackCooldownTimer;

    private Transform chaseTarget;
    private Health chaseTargetHealth;

    private Health health;
    private float deathDelay = 0.1f;

    private bool isAttacking;
    private bool isDeathAnimationPlayed = false;
    private float deathTimer = 0f;
    private float deathTimerMax = 10f;

    private enum State {
        Wander,
        Chase,
        Dead,
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        health.Initialize(enemyTypeSO.maxHealth);

        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemyTypeSO.wanderSpeed;
        activeState = State.Wander;

        entityFaction = GetComponent<AiTarget>().GetFaction();

        attackCooldownTimer = 0f;
    }

    private void OnEnable()
    {
        health.OnDied += Health_OnDied;

        zombieAnimatorFacade.AttackHit += ZombieAnimatorFacade_AttackHit;
        zombieAnimatorFacade.AttackEnd += ZombieAnimatorFacade_AttackEnd;
    }

    private void OnDisable()
    {
        health.OnDied -= Health_OnDied;

        zombieAnimatorFacade.AttackHit -= ZombieAnimatorFacade_AttackHit;
        zombieAnimatorFacade.AttackEnd -= ZombieAnimatorFacade_AttackEnd;
    }


    private void Update()
    {
        switch (activeState)
        {
            default:
            case State.Wander:
                HandleWanderState();
                break;
            case State.Chase:
                HandleChaseState();
                break;
            case State.Dead:
                HandleDeadState();
                break;
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleWanderState()
    {
        // try to detect
        if (detectionTimer <= 0)
        {
            Collider[] hits = GetHitsSortedByDistance(transform.position, enemyTypeSO.detectRadius, targetableMask);

            foreach (Collider hit in hits)
            {
                AiTarget target = hit.GetComponentInParent<AiTarget>();
                if (target == null) continue;

                // avoid self kill
                if (hit.transform.root == transform.root) continue;

                if (target.GetFaction() == entityFaction) continue;

                chaseTargetHealth = target.GetComponentInParent<Health>();
                if (chaseTargetHealth == null) continue;
                if (chaseTargetHealth.IsDead) continue;

                chaseTarget = target.transform;

                SetStateChase();

                detectionTimer = detectionCooldown;

                return;
            }

            detectionTimer = detectionCooldown;
        }
        detectionTimer -= Time.deltaTime;


        if (!agent.isOnNavMesh)
        {
            Debug.LogError("[EnemyAi] Agent is NOT on NavMesh!");
            return;
        }

        if (isIdling)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isIdling = false;
                agent.isStopped = false;
            }
            else
            {
                return;
            }
        }

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            if (UnityEngine.Random.value < 0.4f)
            {
                isIdling = true;
                idleTimer = UnityEngine.Random.Range(enemyTypeSO.idleTimeMin, enemyTypeSO.idleTimeMax);
                agent.ResetPath();
                agent.isStopped = true;

                return;
            }

            if (TryGetRandomPointOnNavMesh(transform.position, enemyTypeSO.wanderRadius, out Vector3 point))
            {
                agent.isStopped = false;
                agent.speed = enemyTypeSO.wanderSpeed;
                agent.SetDestination(point);
            }
        }

    }

    private void HandleChaseState()
    {

        if (chaseTarget == null)
        {
            SetStateWander();

            return;
        }

        if (!TryGetTargetSqrDistance(out float sqrTargetDistance))
        {
            SetStateWander();

            return;
        }

        if (!IsTargetInDetectRange(sqrTargetDistance))
        {
            SetStateWander();

            return;
        }

        if (isAttacking)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        if (IsTargetInAttackRange(sqrTargetDistance))
        {
            agent.isStopped = true;
            agent.ResetPath();

            if (IsAttackCooldownReady() && chaseTargetHealth != null && !isAttacking)
            {

                // check before attack
                if (chaseTargetHealth.IsDead)
                {
                    SetStateWander();

                    return;
                }

                StartAttack();
            }

            // dont chase because we are in attack range
            return; 
        }

        if (agent.isStopped)
        {
            agent.isStopped = false;
            chaseDistanceCheckTimer = 0f;
        }

        if (chaseDistanceCheckTimer <= 0f)
        {
            agent.SetDestination(chaseTarget.position);
            chaseDistanceCheckTimer = chaseDistanceCheckCooldown;
        }
        else
        {
            chaseDistanceCheckTimer -= Time.deltaTime;
        }
    }

    private void HandleDeadState()
    {
        if (!isDeathAnimationPlayed)
        {
            zombieAnimatorFacade.PlayDeath();
            isDeathAnimationPlayed = true;
        }

        if (deathTimer >= deathTimerMax)
        {
            Destroy(gameObject);
        }

        deathTimer += Time.deltaTime;
    }

    private bool TryGetRandomPointOnNavMesh(Vector3 center, float radius, out Vector3 result)
    {
        // try more times to not get invalid position
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = center + UnityEngine.Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(random, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;

                return true;
            }
        }


        result = center;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyTypeSO == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyTypeSO.detectRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, enemyTypeSO.wanderRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemyTypeSO.attackRange);
    }

    private void SetStateWander()
    {
        CancelAttack();
        isIdling = false;
        agent.isStopped = false;

        agent.ResetPath();
        agent.speed = enemyTypeSO.wanderSpeed;
        chaseTarget = null;
        chaseTargetHealth = null;
        activeState = State.Wander;
    }

    private void SetStateChase()
    {
        isIdling = false;
        agent.isStopped = false;

        agent.speed = enemyTypeSO.chaseSpeed;
        agent.SetDestination(chaseTarget.position);
        chaseDistanceCheckTimer = chaseDistanceCheckCooldown;
        activeState = State.Chase;
    }

    private void SetStateDead()
    {
        CancelAttack();
        isIdling = false;
        agent.isStopped = false;
        agent.ResetPath();
        chaseTarget = null;
        chaseTargetHealth = null;
        activeState = State.Dead;
    }

    private bool TryGetTargetSqrDistance(out float sqrTargetDistance)
    {
        sqrTargetDistance = 0f;
        if (chaseTarget == null) return false;

        Vector3 delta = chaseTarget.position - transform.position;
        delta.y = 0f;

        sqrTargetDistance = delta.sqrMagnitude;
        return true;
    }

    private bool IsTargetInDetectRange(float sqrTargetDistance)
    {
        float detectSqr = enemyTypeSO.detectRadius * enemyTypeSO.detectRadius;
        return sqrTargetDistance <= detectSqr;
    }

    private bool IsTargetInAttackRange(float sqrTargetDistance)
    {
        float attackSqr = enemyTypeSO.attackRange * enemyTypeSO.attackRange;
        return sqrTargetDistance <= attackSqr;
    }

    private bool IsAttackCooldownReady()
    {
        return attackCooldownTimer <= 0;
    }

    private void Health_OnDied(object sender, System.EventArgs e)
    {
        SetStateDead();
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(deathDelay);

        Destroy(gameObject);
    }

    private void StartAttack()
    {

        if (chaseTargetHealth == null) return;

        if (chaseTargetHealth.IsDead)
        {
            SetStateWander();
            return;
        }

        isAttacking = true;
        agent.isStopped = true;

        zombieAnimatorFacade.PlayAttack();
    }

    private void ZombieAnimatorFacade_AttackHit()
    {
        if (!isAttacking) return;

        if (chaseTargetHealth != null && !chaseTargetHealth.IsDead)
        {
            float hitRange = enemyTypeSO.attackRange + enemyTypeSO.attackRangeBonus;
            float hitSqr = hitRange * hitRange;

            if (TryGetTargetSqrDistance(out float sqrDist) && sqrDist <= hitSqr)
            {
                chaseTargetHealth.TakeDamage(enemyTypeSO.attackDamage);
            }
        }
    }

    private void ZombieAnimatorFacade_AttackEnd()
    {
        if (!isAttacking) return;
        EndAttack();
    }

    private void EndAttack()
    {
        isAttacking = false;
        agent.isStopped = false;

        attackCooldownTimer = enemyTypeSO.attackCooldown;

        if (chaseTargetHealth == null || chaseTargetHealth.IsDead)
            SetStateWander();
    }

    private void CancelAttack()
    {
        isAttacking = false;

        if (agent != null)
            agent.isStopped = false;
    }

    private Collider[] GetHitsSortedByDistance(Vector3 origin, float radius, LayerMask mask)
    {
        var hits = Physics.OverlapSphere(origin, radius, mask);

        Array.Sort(hits, (a, b) =>
        {
            float da = (a.transform.position - origin).sqrMagnitude;
            float db = (b.transform.position - origin).sqrMagnitude;
            return da.CompareTo(db);
        });

        return hits;
    }
}
