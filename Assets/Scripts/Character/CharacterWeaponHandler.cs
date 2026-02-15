using System.Collections;
using UnityEngine;

public class CharacterWeaponHandler
{
    private WeaponTypeSO weaponTypeSO;
    private Transform weaponSocket;
    private Transform pistolSocketIdle;
    private Transform rifleSocketIdle;

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
        Transform rifleSocketIdle)
    {
        this.runner = runner;
        this.weaponSocket = weaponSocket;
        this.pistolSocketIdle = pistolSocketIdle;
        this.rifleSocketIdle = rifleSocketIdle;
    }


    public void InstantiateWeapon(WeaponTypeSO weaponTypeSO)
    {
        if (weaponTypeSO == null) return;
        this.weaponTypeSO = weaponTypeSO;

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

        currentMagazineAmmo = weaponTypeSO.magazineCapacity;
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
        if (isReloading) return;
        if (currentMagazineAmmo >= weaponTypeSO.magazineCapacity) return;

        reloadRoutine = runner.StartCoroutine(ReloadRoutine());
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
    }
}
