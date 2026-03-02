using GLTFast.Schema;
using UnityEngine;

public sealed class CharacterMeleeWeaponHandler
{
    private readonly CharacterCore characterCore;
    private readonly Transform weaponSocket;
    private readonly Transform meleeIdleShoulderPosition;
    private readonly CharacterAnimatorFacade characterAnimatorFacade;

    private Transform meleeWeapon;

    public bool HasMeleeWeapon => meleeWeapon != null;
    public bool MeleeWeaponInHand { get; private set; }
    public bool MeleeWeaponPositionPending { get; private set; }
    public WeaponItemSO MeleeWeaponItemSO { get; private set; }

    public Transform MeleeWeaponTransform => meleeWeapon;

    public CharacterMeleeWeaponHandler(
        CharacterCore characterCore,
        Transform weaponSocket,
        Transform meleeIdleShoulderPosition,
        CharacterAnimatorFacade characterAnimatorFacade)
    {
        this.characterCore = characterCore;
        this.weaponSocket = weaponSocket;
        this.meleeIdleShoulderPosition = meleeIdleShoulderPosition;
        this.characterAnimatorFacade = characterAnimatorFacade;

        SubscribeToAnimatorEvents();
    }

    /// <summary>
    /// Instantiates a new melee weapon from given prefab.
    /// Automatically places it in idle position.
    /// </summary>
    public void InstantiateMeleeWeapon(WeaponItemSO meleeWeaponItemSO)
    {
        if (meleeWeaponItemSO == null) return;
        if (meleeWeaponItemSO.useMelee == false) return;
        if (meleeWeaponItemSO.meleeWeaponTypeSO == null) return;
        if (meleeIdleShoulderPosition == null) return;


        Debug.Log("InstantiateMeleeWeapon: InstantiateMeleeWeapon");
        MeleeWeaponItemSO = meleeWeaponItemSO;

        meleeWeapon = Object.Instantiate(meleeWeaponItemSO.meleeWeaponTypeSO.prefab);
        SetMeleeInHand(false);
    }

    /// <summary>
    /// Attempts to equip melee weapon into hand.
    /// Uses animation if available.
    /// </summary>
    public void TryEquip()
    {
        if (meleeWeapon == null) return;

        if (characterAnimatorFacade == null)
        {
            SetMeleeInHand(true);
            return;
        }

        MeleeWeaponPositionPending = true;
        characterAnimatorFacade.PlayMeleeEquip();
    }

    /// <summary>
    /// Attempts to disarm melee weapon.
    /// Uses animation if available.
    /// </summary>
    public void TryDisarm()
    {
        if (meleeWeapon == null) return;

        if (characterAnimatorFacade == null)
        {
            SetMeleeInHand(false);
            return;
        }

        MeleeWeaponPositionPending = true;
        characterAnimatorFacade.PlayMeleeDisarm();
    }

    /// <summary>
    /// Called from animation event when equip snap moment occurs.
    /// </summary>
    public void OnEquipSnapEvent()
    {
        if (meleeWeapon == null) return;
        SetMeleeInHand(true);
    }

    /// <summary>
    /// Called from animation event when disarm snap moment occurs.
    /// </summary>
    public void OnDisarmSnapEvent()
    {
        if (meleeWeapon == null) return;
        SetMeleeInHand(false);
    }

    private void SetMeleeInHand(bool inHand)
    {
        if (meleeWeapon == null) return;

        Transform target = inHand ? weaponSocket : meleeIdleShoulderPosition;
        if (target == null) return;

        meleeWeapon.SetParent(target, false);
        meleeWeapon.localPosition = Vector3.zero;
        meleeWeapon.localRotation = Quaternion.identity;

        MeleeWeaponInHand = inHand;
        MeleeWeaponPositionPending = false;
    }

    /// <summary>
    /// Cleanup method for character death or despawn.
    /// </summary>
    public void Dispose()
    {
        UnsubscribeFromAnimatorEvents();
    }

    private void SubscribeToAnimatorEvents()
    {
        if (characterAnimatorFacade == null) return;

        characterAnimatorFacade.OnMeleeEquiped += OnMeleeEquipped;
        characterAnimatorFacade.OnMeleeDisarm += OnMeleeDisarmed;
        characterAnimatorFacade.OnMeleeAttackHit += CharacterAnimatorFacade_OnMeleeAttackHit;
    }

    private void UnsubscribeFromAnimatorEvents()
    {
        if (characterAnimatorFacade == null) return;

        characterAnimatorFacade.OnMeleeEquiped -= OnMeleeEquipped;
        characterAnimatorFacade.OnMeleeDisarm -= OnMeleeDisarmed;
        characterAnimatorFacade.OnMeleeAttackHit -= CharacterAnimatorFacade_OnMeleeAttackHit;
    }

    private void CharacterAnimatorFacade_OnMeleeAttackHit()
    {
        characterCore.TryFinalizeTwoStepAction();
    }

    private void OnMeleeEquipped()
    {
        SetMeleeInHand(true);
    }

    private void OnMeleeDisarmed()
    {
        SetMeleeInHand(false);
    }

    public void PlayMeleeAttackAnimation()
    {
        if (!HasMeleeWeapon) return;

        // If we are mid transition, do nothing
        if (MeleeWeaponPositionPending) return;

        // Ensure weapon is in hand before attacking
        if (!MeleeWeaponInHand)
        {
            TryEquip();
            return;
        }

        // No animator fallback: no animation, but you may still trigger hit logic elsewhere
        if (characterAnimatorFacade == null) return;

        characterAnimatorFacade.PlayMeleeAttack();
    }

    public WeaponRuntimeState SwapCurrentWeaponWithWeaponItem(WeaponItemSO weaponItemSO)
    {
        if (weaponItemSO == null) return null;

        // we get a snapshot of current weapon 
        WeaponRuntimeState previousWeaponRuntimeState = SnapshotWeapon();
        if (previousWeaponRuntimeState == null) return null;

        // we destory current weapon object
        DestroyMeleeWeapon();

        // we instantiate new
        InstantiateMeleeWeapon(weaponItemSO);

        return previousWeaponRuntimeState;
    }

    private WeaponRuntimeState SnapshotWeapon()
    {
        if (HasMeleeWeapon == false) return null;

        return new WeaponRuntimeState(MeleeWeaponItemSO, 0);
    }

    private void DestroyMeleeWeapon()
    {
        if (meleeWeapon == null) return;

        Object.Destroy(meleeWeapon.gameObject);
        meleeWeapon = null;
        MeleeWeaponItemSO = null;
        MeleeWeaponInHand = false;
        MeleeWeaponPositionPending = false;
    }
}
