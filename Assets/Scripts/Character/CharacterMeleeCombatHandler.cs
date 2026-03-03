using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMeleeCombatHandler
{
    private readonly CharacterCore characterCore;
    private readonly NavMeshAgent agent;
    private readonly CharacterMeleeWeaponHandler characterMeleeWeaponHandler;
    private readonly CharacterAnimatorFacade characterAnimatorFacade;
    private readonly CharacterMoveHandler characterMoveHandler;

    private float meleeCombatRepathInterval;
    private float nextMeleeCombatRepathTime = 0f;

    // --- Attack state ---
    private bool attackInProgress = false;
    private float nextAttackTime = 0f;

    // --- Hit settings
    private const float SphereHeight = 1.5f;
    private const float SphereForwardFactor = 0.5f;

    public CharacterMeleeCombatHandler(
        CharacterCore characterCore,
        NavMeshAgent agent,
        CharacterMeleeWeaponHandler characterMeleeWeaponHandler,
        float meleeCombatRepathInterval,
        CharacterAnimatorFacade characterAnimatorFacade,
        CharacterMoveHandler characterMoveHandler)
    {
        this.characterCore = characterCore;
        this.agent = agent;
        this.characterMeleeWeaponHandler = characterMeleeWeaponHandler;
        this.meleeCombatRepathInterval = meleeCombatRepathInterval;
        this.characterAnimatorFacade = characterAnimatorFacade;
        this.characterMoveHandler = characterMoveHandler;

        SubscirbeForEvents();
    }
    public void TryToMeleeAttack()
    {
        if (characterCore.AiTarget == null) return;
        if (!characterCore.HasMeleeWeapon()) return;
        if (!characterMeleeWeaponHandler.MeleeWeaponInHand) return;

        if (characterCore.AiTarget.TryGetComponent(out Health health) && health.IsDead)
        {
            characterCore.ClearAttackTarget();
            attackInProgress = false;
            return;
        }

        Vector3 targetPos = characterCore.AiTarget.GetAimPoint().position;

        float range = characterMeleeWeaponHandler.MeleeWeaponItemSO.meleeWeaponTypeSO.range;

        if (!GeneralHelper.IsInRange(characterCore.transform.position, targetPos, range))
        {
            if (Time.time >= nextMeleeCombatRepathTime)
            {
                nextMeleeCombatRepathTime = Time.time + meleeCombatRepathInterval;
                characterMoveHandler.RunTo(targetPos);
            }
            return;
        }

        characterMoveHandler.RotateTowardsTarget(targetPos);

        Attack();
    }

    private void Attack()
    {
        if (attackInProgress) return;
        if (Time.time < nextAttackTime) return;   // cooldown guard

        if (!characterMeleeWeaponHandler.MeleeWeaponInHand) return;

        attackInProgress = true;

        characterAnimatorFacade.PlayMeleeAttack();
    }
    private void CommitDamage()
    {
        if (!attackInProgress) return;

        var weaponType = characterMeleeWeaponHandler.MeleeWeaponItemSO?.meleeWeaponTypeSO;
        if (weaponType == null) return;

        float damage = weaponType.damage;

        MeleeWeapon weapon = characterMeleeWeaponHandler.MeleeWeapon;
        if (weapon == null) return;

        List<Health> targets = weapon.GetHitTargets(characterCore.transform.root);
        if (targets.Count == 0) return;

        for (int i = 0; i < targets.Count; i++)
        {
            Health health = targets[i];
            if (health == null) continue;

            health.TakeDamage(damage);
        }
    }

    private void WaitForAttackCooldown()
    {
        var weaponType = characterMeleeWeaponHandler.MeleeWeaponItemSO?.meleeWeaponTypeSO;
        if (weaponType == null)
        {
            nextAttackTime = Time.time + 0.5f;
            return;
        }

        float cd = weaponType.hitCooldown;
        if (cd < 0f) cd = 0f;
        nextAttackTime = Time.time + cd;
    }

    private void SubscirbeForEvents()
    {
        characterAnimatorFacade.OnMeleeAttackHit += CharacterAnimatorFacade_OnMeleeAttackHit;
        characterAnimatorFacade.OnMeleeAttackEnd += CharacterAnimatorFacade_OnMeleeAttackEnd;
    }

    private void CharacterAnimatorFacade_OnMeleeAttackEnd()
    {
        if (!attackInProgress) return;

        attackInProgress = false;
        WaitForAttackCooldown();
    }

    private void CharacterAnimatorFacade_OnMeleeAttackHit()
    {
        CommitDamage();
    }
}
