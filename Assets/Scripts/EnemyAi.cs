using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AiTarget))]
public class EnemyAi : MonoBehaviour
{
    // contains 
    //public Transform prefab;
    //public string enemyTypeName;
    //public float wanderSpeed;
    //public float detectRadius;
    [SerializeField] EnemyTypeSO enemyTypeSO;
    [SerializeField] private LayerMask targetableMask;

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

    private void Update()
    {
        //switch (activeState)
        //{
        //    default:
        //    case State.Wander:
        //        HandleWanderState();
        //        break;
        //    case State.Chase:
        //        HandleChaseState();
        //        break;
        //    case State.Dead:
        //        HandleDeadState();
        //        break;
        //}

        //if (attackCooldownTimer > 0f)
        //{
        //    attackCooldownTimer -= Time.deltaTime;
        //}
    }

    private void HandleWanderState()
    {
        // try to detect
        if (detectionTimer <= 0)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, enemyTypeSO.detectRadius, targetableMask);

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
            if (Random.value < 0.4f)
            {
                isIdling = true;
                idleTimer = Random.Range(enemyTypeSO.idleTimeMin, enemyTypeSO.idleTimeMax);
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

        if (IsTargetInAttackRange(sqrTargetDistance))
        {
            agent.isStopped = true;
            agent.ResetPath();

            if (IsAttackCooldownReady() && chaseTargetHealth != null)
            {
                
                // check before attack
                if (chaseTargetHealth.IsDead)
                {
                    SetStateWander();

                    return;
                }

                chaseTargetHealth.TakeDamage(enemyTypeSO.attackDamage);

                // check after attack
                if (chaseTargetHealth.IsDead)
                {
                    SetStateWander();

                    return;
                }

                attackCooldownTimer = enemyTypeSO.attackCooldown;
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

    }

    private bool TryGetRandomPointOnNavMesh(Vector3 center, float radius, out Vector3 result)
    {
        // try more times to not get invalid position
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = center + Random.insideUnitSphere * radius;
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

    private void OnEnable()
    {
        health.OnDied += Health_OnDied;
    }

    private void OnDisable()
    {
        health.OnDied -= Health_OnDied;
    }

    private void Health_OnDied(object sender, System.EventArgs e)
    {
        Destroy(gameObject);
    }
}
