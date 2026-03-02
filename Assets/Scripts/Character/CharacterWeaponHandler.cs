using System;
using System.Collections;
using UnityEngine;

public class CharacterWeaponHandler
{
    public readonly struct AmmoInfo
    {
        public int CurrentAmmo { get; }
        public int TotalAmmo { get; }
        public int MagazineSize { get; }

        public AmmoInfo(int currentAmmo, int totalAmmo, int magazineSize)
        {
            CurrentAmmo = currentAmmo;
            TotalAmmo = totalAmmo;
            MagazineSize = magazineSize;
        }
    }

    public event Action OnWeaponChanged;
    public event Action OnAmmoChanged;

    public class WeaponDataEventArgs : EventArgs
    {
        public int ammo;
        public int totalAmmo;
        public WeaponTypeSO weaponTypeSO;
    }

    private WeaponItemSO weaponItemSO;
    private WeaponTypeSO weaponTypeSO;
    private Transform weaponSocket;
    private Transform pistolSocketIdle;
    private Transform rifleSocketIdle;
    private Inventory inventory;

    private readonly MonoBehaviour runner;

    // Weapon runtime
    private Transform weaponTransform;
    private Weapon weapon;
    private float weaponCooldown;
    private bool isWeaponPrepared = false;

    // Ammo
    private int currentMagazineAmmo = 0;
    private bool isReloading = false;
    private Coroutine reloadRoutine;

    public CharacterWeaponHandler(
        MonoBehaviour runner,
        Transform weaponSocket,
        Transform pistolSocketIdle,
        Transform rifleSocketIdle,
        Inventory inventory
        )
    {
        this.runner = runner;
        this.weaponSocket = weaponSocket;
        this.pistolSocketIdle = pistolSocketIdle;
        this.rifleSocketIdle = rifleSocketIdle;
        this.inventory = inventory;
    }


    public void InstantiateWeapon(WeaponItemSO weaponItemSO, WeaponRuntimeState runtimeState = null)
    {
        if (weaponItemSO == null) return;
        if (weaponItemSO.weaponTypeSO == null) return;

        this.weaponItemSO = weaponItemSO;
        this.weaponTypeSO = weaponItemSO.weaponTypeSO;

        Transform weaponPoistion;

        switch (weaponTypeSO.weaponType)
        {
            default:
            case WeaponType.Pistol:
                weaponPoistion = pistolSocketIdle;
                break;
            case WeaponType.Rifle:
                weaponPoistion = rifleSocketIdle;
                break;
        }

        weaponTransform = UnityEngine.Object.Instantiate(weaponTypeSO.prefab, weaponPoistion);
        weaponTransform.localPosition = Vector3.zero;
        weaponTransform.localRotation = Quaternion.identity;

        weapon = weaponTransform.GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogError("Weapon prefab does not have Weapon script component");
        }


        // If we have runtime state (e.g. from inventory stack), restore mag ammo and do NOT consume ammo.
        if (runtimeState != null)
        {
            currentMagazineAmmo = Mathf.Clamp(runtimeState.CurrentMagazineAmmo, 0, weaponTypeSO.magazineCapacity);
        }
        else
        {
            // default behavior: start with full mag, 
            currentMagazineAmmo = weaponTypeSO.magazineCapacity;

            if (inventory != null && weaponTypeSO.requiredAmmoItemSO != null)
            {
                currentMagazineAmmo = weaponTypeSO.magazineCapacity;

            }
        }

        OnWeaponChanged?.Invoke();
        OnAmmoChanged?.Invoke();
    }

    public void PrepareWeapon()
    {
        if (weaponTypeSO == null) return;
        if (weaponSocket == null) return;
        if (isWeaponPrepared) return;
        if (weaponTransform == null) return;

        if (weapon == null)
        {
            Debug.LogError("Weapon instance does not have Weapon script component");
            return;
        }

        weaponTransform.SetParent(weaponSocket, worldPositionStays: false);
        weaponTransform.localPosition = Vector3.zero;
        weaponTransform.localRotation = Quaternion.identity;

        WeaponPositionInHand weaponPositionInHand = weaponTransform.GetComponentInChildren<WeaponPositionInHand>();
        if (weaponPositionInHand != null)
        {
            weaponTransform.localPosition += weaponPositionInHand.transform.localPosition;
            weaponTransform.localRotation = Quaternion.Inverse(weaponPositionInHand.transform.localRotation);
        }

        weaponCooldown = 0f;
        isWeaponPrepared = true;
    }


    public void HolsterWeapon()
    {
        if (weaponTransform == null) return;
        if (isWeaponPrepared == false) return;

        switch (weaponTypeSO.weaponType)
        {
            default:
            case WeaponType.Pistol:
                weaponTransform.SetParent(pistolSocketIdle, worldPositionStays: false);
                weaponTransform.localPosition = Vector3.zero;
                weaponTransform.localRotation = Quaternion.identity;
                break;
            case WeaponType.Rifle:
                weaponTransform.SetParent(rifleSocketIdle, worldPositionStays: false);
                weaponTransform.localPosition = Vector3.zero;
                weaponTransform.localRotation = Quaternion.identity;
                break;
        }

        isWeaponPrepared = false;
    }

    public void TryStartReload()
    {
        if (weaponTypeSO == null) return;
        if (weapon == null) return;
        if (inventory == null) return;
        if (isReloading) return;
        if (currentMagazineAmmo >= weaponTypeSO.magazineCapacity) return;

        if (weaponTypeSO.requiredAmmoItemSO == null) return;

        int available = inventory.GetTotalAmount(weaponTypeSO.requiredAmmoItemSO);
        if (available <= 0) return;

        reloadRoutine = runner.StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        weapon.PlayReload();

        yield return new WaitForSeconds(weaponTypeSO.reloadTime);

        if (weaponTypeSO == null || inventory == null || weaponTypeSO.requiredAmmoItemSO == null)
        {
            isReloading = false;
            reloadRoutine = null;
            yield break;
        }

        int missingAmmo = weaponTypeSO.magazineCapacity - currentMagazineAmmo;

        if (missingAmmo > 0)
        {
            int taken = inventory.TryConsumeUpToAndGetRemaining(
                weaponTypeSO.requiredAmmoItemSO,
                missingAmmo,
                out int _,
                out int remainingInInventory);

            currentMagazineAmmo += taken;
        }

        isReloading = false;
        reloadRoutine = null;
        OnAmmoChanged?.Invoke();
    }

    public void CancelReload()
    {
        if (reloadRoutine != null)
        {
            runner.StopCoroutine(reloadRoutine);
            reloadRoutine = null;
        }
        isReloading = false;
    }

    public void TickWeaponCooldown(float deltaTime)
    {
        if (weaponCooldown > 0f)
            weaponCooldown -= deltaTime;
    }

    public Weapon GetWeapon() { return weapon; }
    public bool GetIsPrepared() { return isWeaponPrepared; }
    public bool GetIsReloading() { return isReloading; }
    public int GetMagazineAmmo() { return currentMagazineAmmo; }

    public bool WeaponIsReadyToShoot()
    {
        if (isWeaponPrepared == false) return false;
        if (weapon == null) return false;
        if (weaponCooldown > 0) return false;
        if (isReloading) return false;

        return true;
    }

    public void UpdateAfterShot()
    {
        weaponCooldown = weaponTypeSO.shootCooldown;
        currentMagazineAmmo -= 1;
        OnAmmoChanged?.Invoke();
    }

    public AmmoInfo GetAmmoInfo()
    {
        if (weaponTypeSO == null) return new AmmoInfo();
        if (weapon == null) return new AmmoInfo();

        int total = 0;
        if (inventory != null && weaponTypeSO.requiredAmmoItemSO != null)
            total = inventory.GetTotalAmount(weaponTypeSO.requiredAmmoItemSO);

        return new AmmoInfo(currentMagazineAmmo, total, weaponTypeSO.magazineCapacity);
    }


    public WeaponRuntimeState SwapCurrentWeaponWithWeaponItem(WeaponItemSO weaponItemSO, WeaponRuntimeState weaponRuntimeState)
    {
        if (weaponItemSO == null) return null;

        // we get a snapshot of current weapon 
        WeaponRuntimeState previousWeaponRuntimeState = SnapshotWeapon();
        if (previousWeaponRuntimeState == null) return null;

        // we destory current weapon object
        weapon.DestroySelf();

        // we instantiate new
        InstantiateWeapon(weaponItemSO, weaponRuntimeState);

        return previousWeaponRuntimeState;
    }

    private WeaponRuntimeState SnapshotWeapon()
    {
        if (weaponTypeSO == null) return null;

        return new WeaponRuntimeState(weaponItemSO, currentMagazineAmmo);
    }

    public WeaponTypeSO GetWeaponTypeSO()
    {
        return weaponTypeSO;
    }

    public WeaponItemSO GetWeaponItemSO()
    {
        return weaponItemSO;
    }
}

public class WeaponRuntimeState
{
    public WeaponItemSO WeaponItemSO { get; }
    public int CurrentMagazineAmmo { get; }

    public WeaponRuntimeState(WeaponItemSO weaponItemSO, int currentAmmo)
    {
        WeaponItemSO = weaponItemSO;
        CurrentMagazineAmmo = currentAmmo;
    }
}
