using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static CharacterCore;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class CharacterCore : MonoBehaviour, IMoveModeProvider
{
    // Events
    public event EventHandler OnKilled;

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

    private float runClickWindow = 0.30f;
    private float stopSpeedThreshold = 0.1f;

    private float lastMoveClickTime;
    private MoveMode currentMoveMode;

    public bool IsRunning => currentMoveMode == MoveMode.Run;

    public float RunSpeed => characterSO.runSpeed;

    public float WalkSpeed => characterSO.walkSpeed;

    float rotateTowardsTargetSpeed = 200f;

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

    public void MoveTo(Vector3 target)
    {
        ClearAttackTarget();

        characterWeaponHandler.HolsterWeapon();
        characterAnimatorFacade?.DisableAim();

        float now = Time.time;
        bool isDoubleClick = (now - lastMoveClickTime) <= runClickWindow;
        lastMoveClickTime = now;

        if (isDoubleClick)
        {
            currentMoveMode = MoveMode.Run;
            ApplyMoveMode();
        }

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
    }

    public void ClearAttackTarget()
    {
        this.aiTarget = null;
        agent.isStopped = false;
        agent.ResetPath();

        ResetRepositionState();
    }

    public void TryToShoot()
    {
        if (aiTarget == null) return;
        if (weaponTypeSO == null) return;
        if (!characterWeaponHandler.WeaponIsReadyToShoot()) return;

        // Reposition when no line of sight
        if (!HasLineOfSightToTarget(aiTarget))
        {
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

    private void AutoRevertRunToWalkIfStopped()
    {
        if (currentMoveMode != MoveMode.Run) return;

        if (!agent.pathPending && agent.velocity.magnitude < stopSpeedThreshold)
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
}
