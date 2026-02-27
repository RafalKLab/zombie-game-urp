using System;
using UnityEngine;
using static CharacterWeaponHandler;

[System.Serializable]
public class CharacterAiStateDefendHelper
{
    [Header("Defend Settings")]
    [SerializeField] private float defendDetectionCooldown = 0.5f;
    [SerializeField] private LayerMask targetableMask;
    [SerializeField] private float arriveDistance = 0.8f;

    private enum DefendSubState
    {
        GoingToDefendTarget,
        Defending,
    }

    private DefendSubState defendSubState;

    private Vector3 defendPosition;
    private bool hasDefendOrder;

    private float detectTimer;

    private BaseManager baseManager;
    private BaseRadar cachedBaseRadar;

    public void SetBaseManager(BaseManager baseManager)
    {
        this.baseManager = baseManager;
        cachedBaseRadar = baseManager != null ? baseManager.GetComponent<BaseRadar>() : null;
    }

    /// <summary>
    /// Base issues a strict defend order. This becomes the single source of truth.
    /// </summary>
    public void EnterDefendPoint(CharacterCore characterCore, Transform ownerTransform, Vector3 point)
    {
        if (characterCore == null || ownerTransform == null) return;

        defendPosition = point;
        hasDefendOrder = true;

        detectTimer = 0f;

        // Decide initial substate based on distance
        if (Vector3.Distance(ownerTransform.position, defendPosition) <= arriveDistance)
            SetSubState(DefendSubState.Defending, characterCore);
        else
            SetSubState(DefendSubState.GoingToDefendTarget, characterCore);
    }

    /// <summary>
    /// Only used when we intentionally cancel defend (e.g. new state, death, suspend).
    /// </summary>
    public void ClearOrder()
    {
        hasDefendOrder = false;
    }

    public void Tick(
        CharacterCore characterCore,
        AiTarget entityAiTarget,
        Transform ownerTransform,
        ref AiTarget hostileAiTarget,
        ref Health hostileTargetHealth)
    {
        if (!hasDefendOrder) return;
        if (characterCore == null) return;
        if (entityAiTarget == null) return;
        if (ownerTransform == null) return;

        // Move/hold logic
        switch (defendSubState)
        {
            case DefendSubState.GoingToDefendTarget:
                TickGoing(characterCore, ownerTransform);
                break;

            case DefendSubState.Defending:
                TickDefending(characterCore, entityAiTarget, ownerTransform, ref hostileAiTarget, ref hostileTargetHealth);
                break;
        }
    }

    private void TickGoing(CharacterCore characterCore, Transform ownerTransform)
    {
        // If we are close enough -> switch to defending
        if (Vector3.Distance(ownerTransform.position, defendPosition) <= arriveDistance)
        {
            SetSubState(DefendSubState.Defending, characterCore);
        }
        // Otherwise we just keep running toward the defendPosition (RunTo already set in SetSubState)
    }

    private void TickDefending(
        CharacterCore characterCore,
        AiTarget entityAiTarget,
        Transform ownerTransform,
        ref AiTarget hostileAiTarget,
        ref Health hostileTargetHealth)
    {
        // If we drift away (pushed, nav glitch), go back
        if (Vector3.Distance(ownerTransform.position, defendPosition) > arriveDistance * 2f)
        {
            SetSubState(DefendSubState.GoingToDefendTarget, characterCore);
            return;
        }

        // Prepare weapon in idle mode while defending

        bool weaponIsPrepared = characterCore.PrepareWeapon();
        if (weaponIsPrepared)
        {
            AmmoInfo ammoInfo = characterCore.GetAmmoInfo();
            if (ammoInfo.CurrentAmmo == 0 && ammoInfo.TotalAmmo == 0)
            {
                BaseSupplyManager supplyManager = baseManager.GetBaseSupplyManager();
                if (supplyManager != null)
                {
                    supplyManager.TrySupplyAmmoFor(characterCore);
                }
            }
        }
        else
        {
            if (characterCore.TryEquipWeaponFromInventory() == false)
            {
                Debug.Log("Character does not have weapon");
            }
        }

        // Detect hostiles periodically
        detectTimer -= Time.deltaTime;
        if (detectTimer > 0f) return;
        detectTimer = defendDetectionCooldown;

        TryAcquireHostileTarget(characterCore, entityAiTarget, ownerTransform, ref hostileAiTarget, ref hostileTargetHealth);

        // --- 1) Validate current hostile target ---
        bool hasValidHostileTarget =
            hostileAiTarget != null &&
            hostileTargetHealth != null &&
            !hostileTargetHealth.IsDead;

        if (hasValidHostileTarget == false)
        {
            if (characterCore.IsAimModeEnabled())
            {
                characterCore.DisableAim();
            }

            characterCore.EnableRifleIdle();
        } else
        {
            if (characterCore.IsAimModeEnabled() == false)
            {
                characterCore.EnableAim();
            }

            characterCore.DisableRifleIdle();
        }
       
    }

    private void SetSubState(DefendSubState newState, CharacterCore characterCore)
    {
        defendSubState = newState;

        switch (defendSubState)
        {
            case DefendSubState.GoingToDefendTarget:
                characterCore.RunTo(defendPosition);
                break;

            case DefendSubState.Defending:
                characterCore.ResetPath();
                break;
        }
    }

    private void TryAcquireHostileTarget(
    CharacterCore characterCore,
    AiTarget entityAiTarget,
    Transform ownerTransform,
    ref AiTarget hostileAiTarget,
    ref Health hostileTargetHealth)
    {
        if (hostileAiTarget != null && hostileTargetHealth != null && !hostileTargetHealth.IsDead)
        {
            //Debug.Log($"[AI][Acquire] Already have target: {hostileAiTarget.name} (id:{hostileAiTarget.GetInstanceID()})");
            return;
        }

        // mamy radar -> uzywamy TYLKO radaru (bez fallbacku)
        if (cachedBaseRadar != null)
        {
            TryAcquireHostileFromBaseRadar(ref hostileAiTarget, ref hostileTargetHealth);
            return;
        }

        // 2) Otherwise scan locally
        float detectRadius = characterCore.GetCharacterSO().enemyDetectRadius;

        Collider[] hits = GetHitsSortedByDistance(ownerTransform.position, detectRadius, targetableMask);

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            AiTarget potential = hit.GetComponentInParent<AiTarget>();
            if (potential == null) continue;

            if (hit.transform.root == ownerTransform.root) continue;
            if (potential.GetFaction() == entityAiTarget.GetFaction()) continue;
            if (FactionRelationsManager.Instance.AreFriendly(
                        potential.GetFaction(),
                        entityAiTarget.GetFaction()))
            {
                continue;
            }

            Health hp = potential.GetComponentInParent<Health>();
            if (hp == null || hp.IsDead) continue;

            hostileAiTarget = potential;
            hostileTargetHealth = hp;

            //Debug.Log($"[AI][Acquire] From LOCAL SCAN -> {hostileAiTarget.name} (id:{hostileAiTarget.GetInstanceID()})");
            return;
        }

        //Debug.Log("[AI][Acquire] No valid targets found");
    }

    private bool TryAcquireHostileFromBaseRadar(ref AiTarget hostileAiTarget, ref Health hostileTargetHealth)
    {
        if (cachedBaseRadar == null) return false;

        if (!cachedBaseRadar.TryRequestContact(out BaseRadar.RadarContact contact)) return false;
        if (contact == null) return false;

        hostileAiTarget = contact.Target;
        hostileTargetHealth = contact.Health;

        return hostileAiTarget != null && hostileTargetHealth != null && !hostileTargetHealth.IsDead;
    }

    private static Collider[] GetHitsSortedByDistance(Vector3 origin, float radius, LayerMask mask)
    {
        Collider[] hits = Physics.OverlapSphere(origin, radius, mask);

        Array.Sort(hits, (a, b) =>
        {
            float da = (a.transform.position - origin).sqrMagnitude;
            float db = (b.transform.position - origin).sqrMagnitude;
            return da.CompareTo(db);
        });

        return hits;
    }
    public void ClearCurrentTarget(ref AiTarget hostileAiTarget, ref Health hostileTargetHealth)
    {
        hostileAiTarget = null;
        hostileTargetHealth = null;
    }
}