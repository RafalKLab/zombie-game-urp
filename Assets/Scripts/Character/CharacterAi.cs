using System;
using UnityEngine;

[RequireComponent(typeof(CharacterCore))]
[RequireComponent(typeof(AiTarget))]
public class CharacterAi : MonoBehaviour, ICharacterController
{
    public bool IsIdle => mainState == MainState.Idle;
    public bool IsDead => mainState == MainState.Dead;
    public bool IsAiSuspended => isAiSuspended;

    [Header("States")]
    [SerializeField] private MainState mainState;

    [Header("Idle Helper")]
    [SerializeField] private CharacterAiStateIdleHelper characterAiStateIdleHelper;

    [Header("Defend Helper")]
    [SerializeField] private CharacterAiStateDefendHelper characterAiStateDefendHelper;

    [Header("Base")]
    [SerializeField] private BaseManager baseManager;

    private CharacterCore characterCore;
    private AiTarget entityAiTarget;

    private Health hostileTargetHealth;
    private AiTarget hostileAiTarget;
    private bool attackTargetWasSet;

    private PlayableCharacter playableCharacter;

    private bool isAiSuspended = false;
    private bool isDead = false;
    private MainState lastMainStateBeforeSuspend;

    private Vector3 defendPointPosition;

    private enum MainState
    {
        Idle,
        Defend,
        Dead,
    }

    private void Awake()
    {
        characterCore = GetComponent<CharacterCore>();
        entityAiTarget = GetComponent<AiTarget>();
        playableCharacter = GetComponent<PlayableCharacter>();

        lastMainStateBeforeSuspend = MainState.Idle;

        if (characterCore != null)
            characterCore.OnKilled += CharacterCore_OnKilled;
    }

    private void Start()
    {
        SubscribeToActiveCharacterManager();

        hostileAiTarget = null;
        hostileTargetHealth = null;
        attackTargetWasSet = false;

        if (FactionBaseRegistry.Instance != null && entityAiTarget != null)
            baseManager = FactionBaseRegistry.Instance.GetBaseManagerByFaction(entityAiTarget.GetFaction());

        if (baseManager != null)
        {
            if (characterAiStateIdleHelper != null)
                characterAiStateIdleHelper.SetBaseManager(baseManager);

            if (characterAiStateDefendHelper != null)
                characterAiStateDefendHelper.SetBaseManager(baseManager);
        }

        SetMainState(MainState.Idle);
    }

    private void Update()
    {
        if (isDead) return;
        if (isAiSuspended) return;

        UpdateCombatTargetAndShooting();

        switch (mainState)
        {
            default:
            case MainState.Idle:
                if (characterAiStateIdleHelper != null)
                    characterAiStateIdleHelper.Tick(characterCore, transform);
                break;

            case MainState.Defend:
                if (characterAiStateDefendHelper != null)
                    characterAiStateDefendHelper.Tick(characterCore, entityAiTarget, transform, ref hostileAiTarget, ref hostileTargetHealth);
                break;

            case MainState.Dead:
                break;
        }
    }

    // ===== Commands =====

    public void CommandIdle()
    {
        

        SetMainState(MainState.Idle);
    }

    public void CommandDefendPoint(Transform point)
    {
        if (isDead) return;

        if (point == null) return;
        CommandDefendPoint(point.position);
    }

    public void CommandDefendPoint(Vector3 point)
    {
        if (isDead) return;

        defendPointPosition = point;
        SetMainState(MainState.Defend);
    }

    // ===== Active character suspend/resume =====

    private void SubscribeToActiveCharacterManager()
    {
        if (ActiveCharacterManager.Instance == null) return;
        ActiveCharacterManager.Instance.OnActiveCharacterChanged += ActiveCharacterManager_OnActiveCharacterChanged;
    }

    private void UnsubscribeFromActiveCharacterManager()
    {
        if (ActiveCharacterManager.Instance == null) return;
        ActiveCharacterManager.Instance.OnActiveCharacterChanged -= ActiveCharacterManager_OnActiveCharacterChanged;
    }

    private void ActiveCharacterManager_OnActiveCharacterChanged(object sender, ActiveCharacterManager.OnActiveCharacterChangedEventArgs e)
    {
        if (e.playableCharacter == null)
        {
            ResumeAiControl();
            return;
        }

        if (playableCharacter == null)
            playableCharacter = GetComponent<PlayableCharacter>();

        if (playableCharacter != null && playableCharacter == e.playableCharacter)
        {
            SuspendAiControl();
            return;
        }

        ResumeAiControl();
    }

    public void RefreshBase()
    {
        baseManager = null;

        if (FactionBaseRegistry.Instance != null && entityAiTarget != null)
            baseManager = FactionBaseRegistry.Instance.GetBaseManagerByFaction(entityAiTarget.GetFaction());

        if (baseManager != null)
        {
            if (characterAiStateIdleHelper != null)
                characterAiStateIdleHelper.SetBaseManager(baseManager);

            if (characterAiStateDefendHelper != null)
                characterAiStateDefendHelper.SetBaseManager(baseManager);
        }
    }

    private void SuspendAiControl()
    {
        if (isAiSuspended) return;

        lastMainStateBeforeSuspend = mainState;
        isAiSuspended = true;

        hostileAiTarget = null;
        hostileTargetHealth = null;
        attackTargetWasSet = false;

        characterCore.ClearAttackTarget();
        characterCore.ResetPath();
    }

    private void ResumeAiControl()
    {
        if (!isAiSuspended)
            return;

        isAiSuspended = false;

        mainState = lastMainStateBeforeSuspend;

        ForceReenterMainState();
    }

    private void ForceReenterMainState()
    {
        ExitMainState(mainState);
        EnterMainState(mainState);
    }

    // ===== Combat upkeep =====

    private void UpdateCombatTargetAndShooting()
    {
        // --- 0) Check if Core reported a rejected target (one-shot event) ---
        if (characterCore.TryGetRejectedAiTarget(out AiTarget rejected))
        {
            // If Core rejected the same target we are currently trying to use, drop it on AI side
            if (rejected != null && hostileAiTarget == rejected)
            {
                // Drop AI memory of this target so DefendHelper can pick a new one next tick
                characterAiStateDefendHelper.ClearCurrentTarget(ref hostileAiTarget, ref hostileTargetHealth);
                hostileAiTarget = null;
                hostileTargetHealth = null;
                attackTargetWasSet = false;
            }
        }

        // --- 1) Validate current hostile target ---
        bool hasValidHostileTarget =
            hostileAiTarget != null &&
            hostileTargetHealth != null &&
            !hostileTargetHealth.IsDead;

        // If no valid target -> ensure Core is cleared and exit
        if (!hasValidHostileTarget)
        {
            hostileAiTarget = null;
            hostileTargetHealth = null;

            if (attackTargetWasSet)
            {
                characterCore.ClearAttackTarget();
                attackTargetWasSet = false;
            }

            return;
        }

        // --- 2) We have a valid target -> try to set it in Core ---
        characterCore.TrySetAttackTarget(hostileAiTarget, false);
        attackTargetWasSet = true;

        // --- 3) Safety: if Core does not hold the target right after setting it, drop it ---
        // (covers any immediate rejection paths)
        if (!characterCore.HasAttackTarget())
        {
            hostileAiTarget = null;
            hostileTargetHealth = null;
            attackTargetWasSet = false;

            // Also clear helper memory so next detection can pick a different contact
            if (characterAiStateDefendHelper != null)
            {
                characterAiStateDefendHelper.ClearCurrentTarget(ref hostileAiTarget, ref hostileTargetHealth);
            }

            return;
        }

        // --- 4) Core accepted the target -> nothing else to do this frame ---
    }

    public void ClearAttackTarget()
    {
        hostileAiTarget = null;
        hostileTargetHealth = null;
        characterCore.ClearAttackTarget();
        attackTargetWasSet = false;
    }

    public void MoveTo(Vector3 target)
    {
        characterCore.MoveTo(target);
    }

    public void SetAttackTarget(AiTarget target)
    {
        hostileAiTarget = target;
        hostileTargetHealth = target != null ? target.GetComponentInParent<Health>() : null;
        characterCore.SetAttackTarget(target);
        attackTargetWasSet = (target != null);
    }

    // ===== State machine =====

    private void SetMainState(MainState newMainState)
    {
        if (mainState == newMainState)
            return;

        ExitMainState(mainState);
        mainState = newMainState;
        EnterMainState(mainState);
    }

    private void EnterMainState(MainState enteredMainState)
    {
        switch (enteredMainState)
        {
            case MainState.Idle:
                hostileAiTarget = null;
                hostileTargetHealth = null;
                attackTargetWasSet = false;

                characterCore.ClearAttackTarget();
                characterCore.HolsterWeapon();

                // Stop defending order when we go idle
                if (characterAiStateDefendHelper != null)
                    characterAiStateDefendHelper.ClearOrder();

                if (characterAiStateIdleHelper != null)
                    characterAiStateIdleHelper.Enter(characterCore, transform);
                break;

            case MainState.Defend:
                if (characterAiStateDefendHelper != null)
                    characterAiStateDefendHelper.EnterDefendPoint(characterCore, transform, defendPointPosition);
                break;

            case MainState.Dead:
                break;
        }
    }

    private void ExitMainState(MainState exitedMainState)
    {
        // No special exits needed right now
    }

    private void CharacterCore_OnKilled(object sender, EventArgs e)
    {
        isDead = true;

        UnsubscribeFromActiveCharacterManager();

        if (characterCore != null)
            characterCore.OnKilled -= CharacterCore_OnKilled;

        // Clear combat state
        hostileAiTarget = null;
        hostileTargetHealth = null;
        attackTargetWasSet = false;

        // Optional: clear defend order
        if (characterAiStateDefendHelper != null)
            characterAiStateDefendHelper.ClearOrder();

        mainState = MainState.Dead;
        enabled = false;
    }
}