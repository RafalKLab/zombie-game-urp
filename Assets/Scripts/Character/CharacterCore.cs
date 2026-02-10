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

    // Inspector references
    [Header("References")]
    [SerializeField] private CharacterSO characterSO;
    [SerializeField] private Transform cameraLookAtPoint;

    [Header("Weapon")]
    [SerializeField] private WeaponTypeSO weaponTypeSO;

    [Header("Weapon Positions")]
    //[SerializeField] private Transform pistolAimHand;
    [SerializeField] private Transform pistolPositionAim;
    [SerializeField] private Transform pistolPositionIdle;

    //[SerializeField] private Transform rifleAimHand;
    [SerializeField] private Transform riflePositionAim;
    [SerializeField] private Transform riflePositionIdle;

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

    // Weapon runtime
    private Transform weaponTransform;
    private Weapon weapon;
    private float weaponCooldown;
    private bool isWeaponPrepared;

    // Ammo
    private int currentMagazineAmmo;
    private bool isReloading;
    private Coroutine reloadRoutine;

    // Targeting / combat state
    private AiTarget aiTarget;

    // Raycast buffer
    private const int raycastBufferSize = 16;
    private RaycastHit[] raycastHits;

    // Layers
    private int envLayer;

    // Services
    private HitscanShooterService hitscanShooterService;

    // Movement
    public enum MoveMode { Walk, Run }

    private float runClickWindow = 0.30f;
    private float stopSpeedThreshold = 0.1f;

    private float lastMoveClickTime;
    private MoveMode currentMoveMode;

    public bool IsRunning => currentMoveMode == MoveMode.Run;

    public float RunSpeed => characterSO.runSpeed;

    public float WalkSpeed => characterSO.walkSpeed;

    private void Awake()
    {
        health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();

        currentMoveMode = MoveMode.Walk;
        ApplyMoveMode();

        health.Initialize(characterSO.maxHealth);

        InstantiateWeapon();
        raycastHits = new RaycastHit[raycastBufferSize];

        envLayer = LayerMask.NameToLayer("Environment");

        hitscanShooterService = new HitscanShooterService(raycastBufferSize, envLayer);
    }

    private void Update()
    {
        AutoRevertRunToWalkIfStopped();

        if (weaponCooldown > 0f)
            weaponCooldown -= Time.deltaTime;

        if (aiTarget != null) TryToShoot();
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
        CancelReload();
        OnKilled?.Invoke(this, EventArgs.Empty);
    }

    public void MoveTo(Vector3 target)
    {
        ClearAttackTarget();

        if (isWeaponPrepared)
            HolsterWeapon();

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


    public void InstantiateWeapon()
    {
        if (weaponTypeSO == null) return;

        Transform weaponPoistion;

        switch (weaponTypeSO.weaponType)
        {
            default:
            case WeaponType.Pistol:
                weaponPoistion = pistolPositionIdle;
                break;
            case WeaponType.Rifle:
                weaponPoistion = riflePositionIdle;
                break;
        }

        weaponTransform = Instantiate(weaponTypeSO.prefab, weaponPoistion);
        weaponTransform.localPosition = Vector3.zero;
        weaponTransform.localRotation = Quaternion.identity;

        currentMagazineAmmo = weaponTypeSO.magazineCapacity;
    }

    public void SetAttackTarget(AiTarget aiTarget)
    {
        this.aiTarget = aiTarget;
        agent.isStopped = true;
        agent.ResetPath();
        PrepareWeapon();

        ResetRepositionState();
    }

    public void ClearAttackTarget()
    {
        this.aiTarget = null;
        agent.isStopped = false;
        agent.ResetPath();

        ResetRepositionState();
    }

    public void PrepareWeapon()
    {
        if (weaponTypeSO == null) return;
        if (isWeaponPrepared) return;

        weapon = weaponTransform.GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogError("Weapon instance does not have Weapon script component");
            return;
        }

        switch (weaponTypeSO.weaponType)
        {
            default:
            case WeaponType.Pistol:
                //pistolAimHand.gameObject.SetActive(true);
                weaponTransform.SetParent(pistolPositionAim, worldPositionStays: false);
                weaponTransform.localPosition = Vector3.zero;
                weaponTransform.localRotation = Quaternion.identity;
                break;
            case WeaponType.Rifle:
                //rifleAimHand.gameObject.SetActive(true);
                weaponTransform.SetParent(riflePositionAim, worldPositionStays: false);
                weaponTransform.localPosition = Vector3.zero;
                weaponTransform.localRotation = Quaternion.identity;
                break;
        }

        weaponCooldown = 0f;

        isWeaponPrepared = true;
    }

    public void HolsterWeapon()
    {
        if (weaponTransform == null) return;
        if (!isWeaponPrepared) return;

        switch (weaponTypeSO.weaponType)
        {
            default:
            case WeaponType.Pistol:
                //pistolAimHand.gameObject.SetActive(false);
                weaponTransform.SetParent(pistolPositionIdle, worldPositionStays: false);
                weaponTransform.localPosition = Vector3.zero;
                weaponTransform.localRotation = Quaternion.identity;
                break;
            case WeaponType.Rifle:
                //rifleAimHand.gameObject.SetActive(false);
                weaponTransform.SetParent(riflePositionIdle, worldPositionStays: false);
                weaponTransform.localPosition = Vector3.zero;
                weaponTransform.localRotation = Quaternion.identity;
                break;
        }


        isWeaponPrepared = false;
    }

    public void TryToShoot()
    {
        if (aiTarget == null) return;
        if (weaponTypeSO == null) return;
        if (isWeaponPrepared == false) return;
        if (weapon == null) return;
        if (weaponCooldown > 0) return;
        if (isReloading) return;

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

        if (currentMagazineAmmo > 0)
        {
            ShootToTarget(targetPos);
            weaponCooldown = weaponTypeSO.shootCooldown;
            currentMagazineAmmo -= 1;
        }
        else
        {
            TryStartReload();
        }
    }

    public void ShootToTarget(Vector3 targetPos)
    {
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
        float rotateSpeed = 400f;

        Vector3 lookDir = targetPos - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDir);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    bool IsFacingTarget(Vector3 targetPos, float maxAngleDeg = 7f)
    {
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.001f) return true;

        float angle = Vector3.Angle(transform.forward, toTarget);
        return angle <= maxAngleDeg;
    }


    private void TryStartReload()
    {
        if (isReloading) return;
        if (currentMagazineAmmo >= weaponTypeSO.magazineCapacity) return;

        reloadRoutine = StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        weapon.PlayReload();

        yield return new WaitForSeconds(weaponTypeSO.reloadTime);

        currentMagazineAmmo += weaponTypeSO.magazineCapacity;

        isReloading = false;
        reloadRoutine = null;
    }

    private void CancelReload()
    {
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
        }
        isReloading = false;
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

}
