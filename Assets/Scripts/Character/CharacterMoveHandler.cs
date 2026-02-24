using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using static CharacterCore;

public class CharacterMoveHandler
{
    private CharacterCore characterCore;
    private NavMeshAgent agent;

    public enum MoveMode { Walk, Run }
    public MoveMode currentMoveMode;

    private float stopSpeedThreshold = 0.1f;
    private float rotateTowardsTargetSpeed = 300f;

    public bool IsRunning => currentMoveMode == MoveMode.Run;

    public CharacterMoveHandler(CharacterCore characterCore, NavMeshAgent agent)
    {
        this.characterCore = characterCore;
        this.agent = agent;

        currentMoveMode = MoveMode.Walk;
        ApplyMoveMode();
    }


    public void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 lookDir = targetPos - characterCore.transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDir);

        characterCore.transform.rotation = Quaternion.RotateTowards(
            characterCore.transform.rotation,
            targetRotation,
            rotateTowardsTargetSpeed * Time.deltaTime
        );
    }

    public bool IsFacingTarget(Vector3 targetPos, float maxAngleDeg = 1f)
    {
        Vector3 toTarget = targetPos - characterCore.transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.001f) return true;

        float angle = Vector3.Angle(characterCore.transform.forward, toTarget);
        return angle <= maxAngleDeg;
    }

    public void ApplyMoveMode()
    {
        agent.speed = (currentMoveMode == MoveMode.Run)
            ? characterCore.GetCharacterSO().runSpeed
            : characterCore.GetCharacterSO().walkSpeed;
    }

    public void AutoRevertRunToWalkIfStopped()
    {
        if (currentMoveMode != MoveMode.Run) return;

        if (agent.pathPending) return;
        if (!agent.hasPath) return;

        bool isAtDestination = agent.remainingDistance <= agent.stoppingDistance;
        bool isNotMoving = agent.velocity.magnitude < stopSpeedThreshold;

        if (isAtDestination && isNotMoving)
        {
            currentMoveMode = MoveMode.Walk;
            ApplyMoveMode();
        }
    }

    public void MoveTo(Vector3 target)
    {
        currentMoveMode = CharacterMoveHandler.MoveMode.Walk;
        ApplyMoveMode();

        agent.SetDestination(target);
    }

    public void RunTo(Vector3 target)
    {
        currentMoveMode = CharacterMoveHandler.MoveMode.Run;
        ApplyMoveMode();

        agent.SetDestination(target);
    }

    public void ResetPath()
    {
        agent.ResetPath();
    }
}
