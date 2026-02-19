using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static CharacterCore;
using static CharacterWeaponHandler;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class CharacterCore : MonoBehaviour, IMoveModeProvider
{
    // Events
    public event EventHandler OnKilled;
    public event Action OnWeaponChanged;
    public event Action OnAmmoChanged;

    public event EventHandler<OnDamagedEventArgs> OnDamaged;
    public class OnDamagedEventArgs : EventArgs
    {
        public float currentHealthNormalized;
    }

    // Inspector references
    [Header("References")]
    [SerializeField] private CharacterSO characterSO;
    [SerializeField] private Transform cameraLookAtPoint;

    [Header("Weapon")]
    [SerializeField] private WeaponTypeSO weaponTypeSO;

    [Header("Weapon Positions")]
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Transform PistolSocketIdle;
    [SerializeField] private Transform RifleSocketIdle;

    [Header("Line of sight")]
    [SerializeField] private Transform eyesPoint;
    [SerializeField] private LayerMask lineOfSightMask;

    [Header("Strafe settings")]
    [SerializeField] private float strafeMaxDistance = 2f;
    [SerializeField] private float strafeStep = 0.5f;
    [SerializeField] private float strafeNavMeshSampleRadius = 0.4f;
    [SerializeField] private float strafeCapsuleExtraRadius = 0.05f;
    [SerializeField] private float peekOvershoot = 0.6f;
    [SerializeField] private float peekOvershootSampleRadius = 0.5f;

    [Header("Shooting settings")]
    [SerializeField] private int maxConsecutiveMisses = 10;

    // Reposition timing
    private float losGraceTime = 2f;
    private float repositionInterval = 1f;
    private int maxRepositionTries = 3;

    private float noLosTimer;
    private float nextRepositionTime;
    private int repositionTries;

    // Cached components
    private NavMeshAgent agent;
    private Health health;
    private Interactor interactor;

    // Targeting / combat state
    private AiTarget aiTarget;

    // Raycast buffer
    private const int raycastBufferSize = 16;
    private RaycastHit[] raycastHits;

    // Layers
    private int envLayer;

    // Services / Helpers
    private HitscanShooterService hitscanShooterService;
    private CharacterWeaponHandler characterWeaponHandler;

    // Animator
    private CharacterAnimatorFacade characterAnimatorFacade;

    // Movement
    public enum MoveMode { Walk, Run }

    private float stopSpeedThreshold = 0.1f;

    private MoveMode currentMoveMode;

    public bool IsRunning => currentMoveMode == MoveMode.Run;

    public float RunSpeed => characterSO.runSpeed;

    public float WalkSpeed => characterSO.walkSpeed;

    float rotateTowardsTargetSpeed = 200f;

    // Shooting settings runtime
    private int consecutiveMisses = 0;
    private bool firstShotPending = false;
    private bool aimingTimerStarted = false;
    private float aimingReadyTime = 0f;


    private void Awake()
    {
        health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();
        interactor = GetComponent<Interactor>();

        currentMoveMode = MoveMode.Walk;
        ApplyMoveMode();

        health.Initialize(characterSO.maxHealth);

        raycastHits = new RaycastHit[raycastBufferSize];

        envLayer = LayerMask.NameToLayer("Environment");

        hitscanShooterService = new HitscanShooterService(raycastBufferSize, envLayer);
        characterWeaponHandler = new CharacterWeaponHandler(this, weaponSocket, PistolSocketIdle, RifleSocketIdle);
        characterWeaponHandler.OnWeaponChanged += CharacterWeaponHandler_OnWeaponChanged;
        characterWeaponHandler.OnAmmoChanged += CharacterWeaponHandler_OnAmmoChanged;

        characterWeaponHandler.InstantiateWeapon(weaponTypeSO);

        characterAnimatorFacade = GetComponent<CharacterAnimatorFacade>();
    }

    private void Update()
    {
        AutoRevertRunToWalkIfStopped();

        characterWeaponHandler.TickWeaponCooldown(Time.deltaTime);

        if (aiTarget != null) TryToShoot();
    }

    private void OnEnable()
    {
        health.OnDied += Health_OnDied;
        health.OnDamaged += Health_OnDamaged; ;
    }

    private void OnDisable()
    {
        health.OnDied -= Health_OnDied;
        health.OnDamaged -= Health_OnDamaged;
    }

    private void Health_OnDied(object sender, System.EventArgs e)
    {
        characterWeaponHandler.CancelReload();
        OnKilled?.Invoke(this, EventArgs.Empty);
    }
    private void Health_OnDamaged(object sender, Health.OnDamagedEventArgs e)
    {
        OnDamaged?.Invoke(this, new OnDamagedEventArgs
        {
            currentHealthNormalized = e.currentHealthNormalized,
        });
    }

    private void CharacterWeaponHandler_OnWeaponChanged()
    {
        OnWeaponChanged?.Invoke();
    }

    private void CharacterWeaponHandler_OnAmmoChanged()
    {
        OnAmmoChanged?.Invoke();
    }

    public void MoveTo(Vector3 target)
    {
        ClearAttackTarget();

        characterWeaponHandler.HolsterWeapon();
        characterAnimatorFacade?.DisableAim();

        currentMoveMode = MoveMode.Walk;
        ApplyMoveMode();

        agent.SetDestination(target);
    }

    public void RunTo(Vector3 target)
    {
        ClearAttackTarget();

        characterWeaponHandler.HolsterWeapon();
        characterAnimatorFacade?.DisableAim();

        currentMoveMode = MoveMode.Run;
        ApplyMoveMode();

        agent.SetDestination(target);
    }

    public void SetAttackTarget(AiTarget aiTarget)
    {
        this.aiTarget = aiTarget;
        agent.isStopped = true;
        agent.ResetPath();
        characterWeaponHandler.PrepareWeapon();
        characterAnimatorFacade?.EnableAim(weaponTypeSO);

        ResetRepositionState();

        firstShotPending = (this.aiTarget != null);
        aimingTimerStarted = false;
    }

    public bool PrepareWeapon()
    {
        characterWeaponHandler.PrepareWeapon();
        characterAnimatorFacade?.EnableAim(weaponTypeSO);

        return characterWeaponHandler.GetIsPrepared();
    }

    public void HolsterWeapon()
    {
        characterWeaponHandler.HolsterWeapon();
        characterAnimatorFacade?.DisableAim();
    }

    public void ClearAttackTarget()
    {
        this.aiTarget = null;
        agent.isStopped = false;
        agent.ResetPath();

        ResetRepositionState();

        firstShotPending = false;
        aimingTimerStarted = false;
    }

    public void TryToShoot()
    {
        if (aiTarget == null) return;
        if (weaponTypeSO == null) return;
        if (!characterWeaponHandler.WeaponIsReadyToShoot()) return;

        // Reposition when no line of sight
        if (!HasLineOfSightToTarget(aiTarget))
        {
            aimingTimerStarted = false;
            noLosTimer += Time.deltaTime;

            if (noLosTimer < losGraceTime)
                return;

            if (repositionTries >= maxRepositionTries)
            {
                ClearAttackTarget();
                return;
            }

            TryRepositionToGainLineOfSight();
            return;
        }

        // mamy LOS -> reset timera
        ResetRepositionState();

        Vector3 targetPos = aiTarget.GetAimPoint().position;

        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }


        // if agent is stopping
        if (agent.velocity.sqrMagnitude > 0.01f) return;

        RotateTowardsTarget(targetPos);

        if (!IsFacingTarget(targetPos))
            return;

        if (firstShotPending)
        {
            if (!aimingTimerStarted)
            {
                aimingTimerStarted = true;
                aimingReadyTime = Time.time + characterSO.aimingTime;
                return; // ta klatka tylko "odpala" timer
            }

            if (Time.time < aimingReadyTime)
                return;

            firstShotPending = false;
            aimingTimerStarted = false;
        }

        if (characterWeaponHandler.GetMagazineAmmo() > 0)
        {
            ShootToTarget(targetPos);
            characterWeaponHandler.UpdateAfterShot();
        }
        else
        {
            characterWeaponHandler.TryStartReload();
        }
    }

    public void ShootToTarget(Vector3 targetPos)
    {
        Weapon weapon = characterWeaponHandler.GetWeapon();
        if (weapon == null) return;

        Transform muzzle = weapon.GetMuzzle();
        if (muzzle == null) return;

        Vector3 origin = muzzle.position;
        Vector3 baseDirection = (targetPos - origin).normalized;

        bool inEffective = IsTargetInWeaponEffectiveRange(targetPos, origin);

        float finalAccuracy = inEffective
            ? weaponTypeSO.accuracy
            : weaponTypeSO.accuracyOutEffectiveRange;

        // pozniej: finalAccuracy *= shooterSkill;

        float spread = Mathf.Lerp(weaponTypeSO.maxSpreadAngle, 0f, finalAccuracy);

        Vector3 direction = AimSpreadService.ApplyConeSpread(baseDirection, spread);

        ShotResult shot = hitscanShooterService.Shoot(origin, direction, weaponTypeSO, transform.root, aiTarget);
        weapon.PlayShot(shot);
        weapon.PlayCooldown();

        if (shot.HitActiveTarget) consecutiveMisses = 0;
        else consecutiveMisses++;

        if (consecutiveMisses >= maxConsecutiveMisses)
        {
            consecutiveMisses = 0;
            ClearAttackTarget();
        }

        if (shot.ActiveTargetKilled)
            ClearAttackTarget();
    }

    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 lookDir = targetPos - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDir);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotateTowardsTargetSpeed * Time.deltaTime
        );
    }

    bool IsFacingTarget(Vector3 targetPos, float maxAngleDeg = 1f)
    {
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.001f) return true;

        float angle = Vector3.Angle(transform.forward, toTarget);
        return angle <= maxAngleDeg;
    }

    private bool HasLineOfSightToTarget(AiTarget target)
    {
        if (target == null) return false;

        Transform aim = target.GetAimPoint();
        if (aim == null) return false;

        if (eyesPoint == null) return false;

        Vector3 origin = eyesPoint.position;
        Vector3 dest = aim.position;

        Vector3 dir = dest - origin;
        float dist = dir.magnitude;
        if (dist < 0.01f) return true;

        Vector3 dirNorm = dir / dist;

        var hits = Physics.RaycastAll(origin, dirNorm, dist, lineOfSightMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.transform.IsChildOf(transform) || transform.IsChildOf(h.collider.transform))
                continue;

            return h.collider.GetComponentInParent<AiTarget>() == target;
        }

        return false;
    }

    private bool HasLineOfSightToTarget(AiTarget target, Vector3 fromCharacterPosition)
    {
        if (target == null) return false;

        Transform aim = target.GetAimPoint();
        if (aim == null) return false;

        if (eyesPoint == null) return false;

        Vector3 eyesOffset = eyesPoint.position - transform.position;

        Vector3 origin = fromCharacterPosition + eyesOffset;
        Vector3 dest = aim.position;

        Vector3 dir = dest - origin;
        float dist = dir.magnitude;
        if (dist < 0.01f) return true;

        Vector3 dirNorm = dir / dist;

        var hits = Physics.RaycastAll(origin, dirNorm, dist, lineOfSightMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.transform.IsChildOf(transform) || transform.IsChildOf(h.collider.transform))
                continue;

            return h.collider.GetComponentInParent<AiTarget>() == target;
        }

        return false;
    }


    public Transform GetCameraLookAtPoint()
    {
        return cameraLookAtPoint;
    }
    private void TryRepositionToGainLineOfSight()
    {
        if (aiTarget == null) return;

        if (Time.time < nextRepositionTime)
            return;

        nextRepositionTime = Time.time + repositionInterval;
        repositionTries++;

        agent.isStopped = false;

        if (TryFindStrafePeekPoint(out Vector3 peekPoint))
        {
            Vector3 dir = peekPoint - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Vector3 overshootCandidate = peekPoint + dir.normalized * peekOvershoot;

                if (UnityEngine.AI.NavMesh.SamplePosition(
                        overshootCandidate,
                        out UnityEngine.AI.NavMeshHit hit,
                        peekOvershootSampleRadius,
                        agent.areaMask))
                {
                    peekPoint = hit.position;
                }
            }

            agent.SetDestination(peekPoint);
            return;
        }

        agent.SetDestination(aiTarget.transform.position);
    }


    private bool IsTargetInWeaponEffectiveRange(Vector3 targetPos, Vector3 origin)
    {
        if (weaponTypeSO == null) return false;

        float range = weaponTypeSO.effectiveRange;
        float sqrDist = (targetPos - origin).sqrMagnitude;
        return sqrDist <= range * range;
    }

    private void ResetRepositionState()
    {
        noLosTimer = 0f;
        nextRepositionTime = 0f;
        repositionTries = 0;
    }

    public bool HasAttackTarget()
    {
        if (aiTarget == null)
            return false;
        else
            return true;
    }

    private void ApplyMoveMode()
    {
        agent.speed = (currentMoveMode == MoveMode.Run)
            ? characterSO.runSpeed
            : characterSO.walkSpeed;
    }

    public bool TrySetAttackTarget(AiTarget newTarget)
    {
        if (aiTarget == newTarget)
            return false;

        SetAttackTarget(newTarget);
        return true;
    }

    private void AutoRevertRunToWalkIfStopped()
    {
        if (currentMoveMode != MoveMode.Run) return;

        if (agent.pathPending) return;
        if (!agent.hasPath) return;

        bool isAtDestination = agent.remainingDistance <= agent.stoppingDistance;
        bool isNotMoving = agent.velocity.magnitude < stopSpeedThreshold;

        if (isAtDestination && isNotMoving)
        {
            currentMoveMode = MoveMode.Walk;
            ApplyMoveMode();
        }
    }

    public bool HasWeapon()
    {
        return weaponTypeSO != null;
    }

    public bool TrySetWeapon(WeaponTypeSO weaponTypeSO)
    {
        if (weaponTypeSO == null) return false;
        if (HasWeapon()) return false;

        this.weaponTypeSO = weaponTypeSO;
        characterWeaponHandler.InstantiateWeapon(weaponTypeSO);

        return true;
    }

    public bool TryInteract()
    {
        if (interactor == null) return false;
        return interactor.TryInteractCurrent();
    }

    public float GetNormalizedHealth()
    {
        return health.GetNormalizedHealth();
    }

    public WeaponTypeSO GetWeaponTypeSO()
    {
        return weaponTypeSO;
    }

    public CharacterSO GetCharacterSO()
    {
        return characterSO;
    }

    public AmmoInfo GetAmmoInfo()
    {
        return characterWeaponHandler.GetAmmoInfo();
    }

    private bool TryFindStrafePeekPoint(out Vector3 bestPoint)
    {
        bestPoint = default;

        if (aiTarget == null) return false;
        if (agent == null) return false;

        Vector3 originPos = transform.position;
        Vector3 targetPos = aiTarget.transform.position;

        Vector3 toTarget = (targetPos - originPos);
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f) return false;
        toTarget.Normalize();

        // lewo/prawo wzgledem celu
        Vector3 left = Vector3.Cross(Vector3.up, toTarget).normalized;
        Vector3 right = -left;

        // parametry kapsuly dla checow
        float radius = agent.radius + strafeCapsuleExtraRadius;
        float height = Mathf.Max(agent.height, radius * 2f);
        Vector3 up = Vector3.up;

        // capsule endpoints (w swiecie) dla pozycji "stop" = candidate
        float bottomOffset = radius;
        float topOffset = height - radius;

        Vector3[] sides = UnityEngine.Random.value < 0.5f
            ? new[] { left, right }
            : new[] { right, left };

        for (float d = strafeStep; d <= strafeMaxDistance + 0.001f; d += strafeStep)
        {
            for (int s = 0; s < sides.Length; s++)
            {
                Vector3 candidateRaw = originPos + sides[s] * d;

                // 1) NavMesh sample (czy w ogole jest gdzie stanac)
                if (!NavMesh.SamplePosition(candidateRaw, out NavMeshHit hit, strafeNavMeshSampleRadius, agent.areaMask))
                    continue;

                Vector3 candidate = hit.position;

                // 2) NavMesh raycast (czy agent moze dojsc do tego miejsca po navmesh bez "sciany")
                if (NavMesh.Raycast(originPos, candidate, out _, agent.areaMask))
                    continue;

                // 3) Kolizje fizyczne (czy nie wpychamy w collider)
                Vector3 p1 = candidate + up * bottomOffset;
                Vector3 p2 = candidate + up * topOffset;

                // Uwaga: uzywam ~0 (wszystkie warstwy) i Ignore triggers, bo to typowy przypadek.
                if (Physics.CheckCapsule(p1, p2, radius, ~0, QueryTriggerInteraction.Ignore))
                    continue;

                // 4) LOS z tej pozycji
                if (!HasLineOfSightToTarget(aiTarget, candidate))
                    continue;

                bestPoint = candidate;
                return true;
            }
        }

        return false;
    }


    public void ResetPath()
    {
        agent.ResetPath();
    }
}
