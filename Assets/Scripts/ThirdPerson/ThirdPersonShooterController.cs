using StarterAssets;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera aimVirtualCamera;
    [SerializeField] private CinemachineBrain brain;

    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;

    [SerializeField] private Transform rigBodyAimTarget;

    [SerializeField] private Rig aimRig;

    [SerializeField] private LayerMask aimLayerMask = new LayerMask();

    // Weapon
    [SerializeField] Transform WeaponPistolSocket;
    [SerializeField] WeaponTypeSO weaponTypeSO;

    [SerializeField] private LayerMask hitscanLayerMask;

    // Weapon runtime
    private Transform weaponTransform;
    private Weapon weapon;
    private float weaponCooldown;
    private bool isWeaponPrepared;

    // Ammo
    private int currentMagazineAmmo;
    private bool isReloading;
    private Coroutine reloadRoutine;

    private StarterAssetsInputs starterAssetsInputs;
    private ThirdPersonController thirdPersonController;
    private Animator animator;

    // Raycast buffer
    private const int raycastBufferSize = 16;

    // Services
    private HitscanShooterService hitscanShooterService;

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();

        hitscanShooterService = new HitscanShooterService(raycastBufferSize, hitscanLayerMask);

        brain.DefaultBlend.Time = 0.1f;
    }

    private void Start()
    {
        InstantiateWeapon();
    }

    private void Update()
    {
        if (weaponCooldown > 0f)
            weaponCooldown -= Time.deltaTime;

        if (starterAssetsInputs.aim && weaponTypeSO != null)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
            aimRig.weight = Mathf.Lerp(aimRig.weight, 1f, Time.deltaTime * 10f);

            Vector3 mouseWorldPosition = Vector3.zero;

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimLayerMask))
            {
                mouseWorldPosition = raycastHit.point;
                rigBodyAimTarget.position = raycastHit.point;


                if (starterAssetsInputs.shoot)
                {
                    TryToShoot(raycastHit.point);
                    starterAssetsInputs.shoot = false;
                }
            }

            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.Sensitivity = aimSensitivity;
            thirdPersonController.SetRotateOnMove(false);

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        } else
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            aimRig.weight = Mathf.Lerp(aimRig.weight, 0f, Time.deltaTime * 10f);

            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.Sensitivity = normalSensitivity;
            thirdPersonController.SetRotateOnMove(true);
        }

        if (starterAssetsInputs.shoot)
        {
            starterAssetsInputs.shoot = false;
        }
    }


    public void InstantiateWeapon()
    {
        if (weaponTypeSO == null) return;

        Transform weaponPoistion;

        switch (weaponTypeSO.weaponType)
        {
            default:
            case WeaponType.Pistol:
                weaponPoistion = WeaponPistolSocket;
                break;
            case WeaponType.Rifle:
                weaponPoistion = WeaponPistolSocket;
                break;
        }

        weaponTransform = Instantiate(weaponTypeSO.prefab, weaponPoistion);
        weaponTransform.localPosition = Vector3.zero;
        weaponTransform.localRotation = Quaternion.identity;

        weapon = weaponTransform.GetComponent<Weapon>();

        currentMagazineAmmo = weaponTypeSO.magazineCapacity;
    }

    public void TryToShoot(Vector3 targetPos)
    {
        if (weaponTypeSO == null) return;
        if (weapon == null) return;
        if (weaponCooldown > 0) return;
        if (isReloading) return;

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


        ShotResult shot = hitscanShooterService.Shoot(origin, direction, weaponTypeSO, transform.root, null);

        weapon.PlayShot(shot);
        weapon.PlayCooldown();
    }

    private bool IsTargetInWeaponEffectiveRange(Vector3 targetPos, Vector3 origin)
    {
        if (weaponTypeSO == null) return false;

        float range = weaponTypeSO.effectiveRange;
        float sqrDist = (targetPos - origin).sqrMagnitude;
        return sqrDist <= range * range;
    }
}
