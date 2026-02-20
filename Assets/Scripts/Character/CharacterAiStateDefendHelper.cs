using System;
using UnityEngine;
using static CharacterWeaponHandler;

[System.Serializable]
public class CharacterAiStateDefendHelper
{
    [Header("Defend Settings")]
    [SerializeField] private float defendDetectionCooldown = 0.5f;
    [SerializeField] private LayerMask targetableMask;
    [SerializeField] private float defendHoldArriveDistance = 0.8f;
    [SerializeField] private DefendSubState defendSubState;


    private enum DefendSubState
    {
        GoingToDefendTarget,
        Defending,
    }

    private Vector3 defendPosition;
    private bool defendPositionIsSet;

    private float defendDetectionTimer;
    private BaseManager baseManager;


    public void SetBaseManager(BaseManager baseManager)
    {
        this.baseManager = baseManager;
    }

    public void EnterDefendHere(CharacterCore characterCore, Transform ownerTransform)
    {
        defendPosition = ownerTransform.position;
        defendPositionIsSet = true;
        defendDetectionTimer = 0f;

        SetDefendSubState(DefendSubState.Defending, characterCore, ownerTransform);
    }

    public void EnterDefendPoint(CharacterCore characterCore, Transform ownerTransform, Vector3 point)
    {
        defendPosition = point;
        defendPositionIsSet = true;
        defendDetectionTimer = 0f;

        SetDefendSubState(DefendSubState.GoingToDefendTarget, characterCore, ownerTransform);
    }

    public void Tick(
        CharacterCore characterCore,
        AiTarget entityAiTarget,
        Transform ownerTransform,
        ref AiTarget hostileAiTarget,
        ref Health hostileTargetHealth)
    {
        if (!defendPositionIsSet)
        {
            return;
        }

        bool weaponIsPrepared = characterCore.PrepareWeapon();
        if (weaponIsPrepared)
        {
            AmmoInfo ammoInfo = characterCore.GetAmmoInfo();
            if (ammoInfo.CurrentAmmo == 0 && ammoInfo.TotalAmmo == 0)
            {
                // TODO: brak amunicji
            }
        }
        else
        {
            // TODO: brak broni
        }

        switch (defendSubState)
        {
            case DefendSubState.GoingToDefendTarget:
                HandleDefendGoingToTarget(characterCore, ownerTransform);
                break;

            case DefendSubState.Defending:
                HandleDefendHolding(characterCore, ownerTransform);
                break;
        }

        if (defendSubState == DefendSubState.Defending)
        {
            defendDetectionTimer -= Time.deltaTime;
            if (defendDetectionTimer <= 0f)
            {
                TryAcquireHostileTarget(characterCore, entityAiTarget, ownerTransform, ref hostileAiTarget, ref hostileTargetHealth);
                defendDetectionTimer = defendDetectionCooldown;
            }
        }
    }

    public void ResetDefendPosition()
    {
        defendPositionIsSet = false;
        defendSubState = DefendSubState.Defending;
    }

    private void SetDefendSubState(DefendSubState newState, CharacterCore characterCore, Transform ownerTransform)
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

    private void HandleDefendGoingToTarget(CharacterCore characterCore, Transform ownerTransform)
    {
        if (Vector3.Distance(ownerTransform.position, defendPosition) <= defendHoldArriveDistance)
        {
            SetDefendSubState(DefendSubState.Defending, characterCore, ownerTransform);
        }
    }

    private void HandleDefendHolding(CharacterCore characterCore, Transform ownerTransform)
    {
        if (Vector3.Distance(ownerTransform.position, defendPosition) > defendHoldArriveDistance * 2f)
        {
            SetDefendSubState(DefendSubState.GoingToDefendTarget, characterCore, ownerTransform);
        }
    }

    private void TryAcquireHostileTarget(
            CharacterCore characterCore,
            AiTarget entityAiTarget,
            Transform ownerTransform,
            ref AiTarget hostileAiTarget,
            ref Health hostileTargetHealth
        )
    {
        if (hostileAiTarget != null && hostileTargetHealth != null && !hostileTargetHealth.IsDead)
            return;

        if (TryAcquireHostileFromBaseRadar(ref hostileAiTarget, ref hostileTargetHealth))
            return;

        float detectRadius = characterCore.GetCharacterSO().enemyDetectRadius;

        Collider[] hits = GetHitsSortedByDistance(ownerTransform.position, detectRadius, targetableMask);

        foreach (Collider hit in hits)
        {
            AiTarget potentialHostileAiTarget = hit.GetComponentInParent<AiTarget>();
            if (potentialHostileAiTarget == null) continue;

            if (hit.transform.root == ownerTransform.root) continue;

            if (potentialHostileAiTarget.GetFaction() == entityAiTarget.GetFaction()) continue;

            Health potentialHostileTargetHealth = potentialHostileAiTarget.GetComponentInParent<Health>();
            if (potentialHostileTargetHealth == null) continue;
            if (potentialHostileTargetHealth.IsDead) continue;

            hostileAiTarget = potentialHostileAiTarget;
            hostileTargetHealth = potentialHostileTargetHealth;
            return;
        }
    }

    private bool TryAcquireHostileFromBaseRadar(ref AiTarget hostileAiTarget, ref Health hostileTargetHealth)
    {
        if (baseManager == null) return false;

        BaseRadar baseRadar = baseManager.GetComponent<BaseRadar>();
        if (baseRadar == null) return false;

        if (!baseRadar.TryRequestContact(out BaseRadar.RadarContact radarContact)) return false;
        if (radarContact == null) return false;

        hostileAiTarget = radarContact.Target;
        hostileTargetHealth = radarContact.Health;

        return hostileAiTarget != null && hostileTargetHealth != null && !hostileTargetHealth.IsDead;
    }

    private static Collider[] GetHitsSortedByDistance(Vector3 origin, float radius, LayerMask mask)
    {
        Collider[] hits = Physics.OverlapSphere(origin, radius, mask);

        Array.Sort(hits, (a, b) =>
        {
            float distanceA = (a.transform.position - origin).sqrMagnitude;
            float distanceB = (b.transform.position - origin).sqrMagnitude;
            return distanceA.CompareTo(distanceB);
        });

        return hits;
    }
}
