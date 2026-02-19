using System;
using UnityEngine;
using static BaseManager;

[RequireComponent(typeof(CharacterCore))]
[RequireComponent(typeof(AiTarget))]
public class CharacterAi : MonoBehaviour, ICharacterController
{
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

    private bool defendPointIsSet;
    private Vector3 defendPointPosition;

    private enum MainState
    {
        Idle,
        Defend,
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

        SetMainState(MainState.Idle);

        hostileAiTarget = null;
        hostileTargetHealth = null;
        attackTargetWasSet = false;

        if (FactionBaseRegistry.Instance != null)
            baseManager = FactionBaseRegistry.Instance.GetBaseManagerByFaction(entityAiTarget.GetFaction());

        if (baseManager != null)
            baseManager.OnBaseDefendRequest += BaseManager_OnBaseDefendRequest;

        if (characterAiStateIdleHelper != null)
            characterAiStateIdleHelper.SetBaseManager(baseManager);
    }

    private void BaseManager_OnBaseDefendRequest(object sender, OnBaseDefendRequestEventArgs e)
    {
        if (isDead)
            return;

        if (isAiSuspended)
            return;

        hostileAiTarget = null;
        hostileTargetHealth = null;
        attackTargetWasSet = false;

        float defendNavmeshSampleMaxDistance = 2f;
        int defendPickAttempts = 12;

        Vector3 pickedPoint;

        bool gotPoint = NavMeshPointPicker.TryGetRandomPointWithMinRadius(
            e.defendPoint.position,
            e.defendRadius,
            0.4f,
            defendNavmeshSampleMaxDistance,
            defendPickAttempts,
            UnityEngine.AI.NavMesh.AllAreas,
            out pickedPoint
        );


        if (gotPoint)
        {
            CommandDefendPoint(pickedPoint);
        }
        else
        {
            CommandDefendPoint(e.defendPoint);
        }
    }


    private void Update()
    {
        if (isDead)
            return;

        if (isAiSuspended)
            return;

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
        }
    }

    public void CommandIdle()
    {

        SetMainState(MainState.Idle);
    }

    public void CommandDefendHere()
    {
        defendPointIsSet = false;

        SetMainState(MainState.Defend);
    }

    public void CommandDefendPoint(Transform point)
    {
        defendPointIsSet = true;
        defendPointPosition = point.position;

        SetMainState(MainState.Defend);
    }

    public void CommandDefendPoint(Vector3 point)
    {
        defendPointIsSet = true;
        defendPointPosition = point;

        SetMainState(MainState.Defend);
    }


    private void SubscribeToActiveCharacterManager()
    {
        if (ActiveCharacterManager.Instance == null) return;

        ActiveCharacterManager.Instance.OnActiveCharacterChanged += ActiveCharacterManager_OnActiveCharacterChanged;
    }

    private void UnsubscribeFromActiveCharacterManager()
    {
        if (ActiveCharacterManager.Instance == null)
            return;

        ActiveCharacterManager.Instance.OnActiveCharacterChanged -= ActiveCharacterManager_OnActiveCharacterChanged;
    }

    private void CharacterCore_OnKilled(object sender, EventArgs e)
    {
        isDead = true;

        UnsubscribeFromActiveCharacterManager();

        if (baseManager != null)
            baseManager.OnBaseDefendRequest -= BaseManager_OnBaseDefendRequest;

        if (characterCore != null)
            characterCore.OnKilled -= CharacterCore_OnKilled;

        enabled = false;
    }

    private void ActiveCharacterManager_OnActiveCharacterChanged(object sender, ActiveCharacterManager.OnActiveCharacterChangedEventArgs e)
    {
        if (e.playableCharacter == null)
        {
            ResumeAiControl();
            return;
        }

        if (playableCharacter == null)
        {
            playableCharacter = GetComponent<PlayableCharacter>();
        }

        if (playableCharacter != null && playableCharacter == e.playableCharacter)
        {
            SuspendAiControl();
            return;
        }

        ResumeAiControl();
    }

    private void SuspendAiControl()
    {
        if (isAiSuspended)
            return;

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
        EnterMainState(mainState);
    }

    private void UpdateCombatTargetAndShooting()
    {
        bool hasValidHostileTarget =
            hostileAiTarget != null &&
            hostileTargetHealth != null &&
            !hostileTargetHealth.IsDead;

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

        characterCore.TrySetAttackTarget(hostileAiTarget);
        attackTargetWasSet = true;
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

                if (characterAiStateIdleHelper != null)
                    characterAiStateIdleHelper.Enter(characterCore, transform);

                if (characterAiStateDefendHelper != null)
                    characterAiStateDefendHelper.ResetDefendPosition();

                break;

            case MainState.Defend:
                if (characterAiStateDefendHelper != null)
                {
                    if (defendPointIsSet)
                    {
                        characterAiStateDefendHelper.EnterDefendPoint(characterCore, transform, defendPointPosition);
                    }
                    else
                    {
                        characterAiStateDefendHelper.EnterDefendHere(characterCore, transform);
                    }
                }
                break;

        }
    }

    private void ExitMainState(MainState exitedMainState)
    {
        switch (exitedMainState)
        {
            case MainState.Idle:
                break;

            case MainState.Defend:
                defendPointIsSet = false;
                defendPointPosition = Vector3.zero;
                break;
        }
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.darkMagenta;
    //    Gizmos.DrawWireSphere(transform.position, characterCore.GetCharacterSO().enemyDetectRadius);
    //}
}
