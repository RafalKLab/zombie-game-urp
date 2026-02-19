using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CharacterAiStateIdleHelper
{
    [Header("Idle Timings")]
    [SerializeField] private float idleStandMin = 3f;
    [SerializeField] private float idleStandMax = 6f;

    [Header("Wander")]
    [SerializeField] private float arriveDistance = 0.5f;
    [SerializeField] private float navmeshSampleMaxDistance = 2f;

    private BaseManager baseManager;
    public void SetBaseManager(BaseManager baseManager)
    {
        this.baseManager = baseManager;
    }

    private enum IdleSubState
    {
        Standing,
        Wandering,
    }

    [SerializeField] private IdleSubState idleSubState;

    private float idleStandingTimer;
    private Vector3 wanderDestination;

    public void Enter(CharacterCore characterCore, Transform ownerTransform)
    {
        SetIdleSubState(IdleSubState.Standing, characterCore, ownerTransform);
    }

    public void Tick(CharacterCore characterCore, Transform ownerTransform)
    {
        switch (idleSubState)
        {
            case IdleSubState.Standing:
                HandleIdleStanding(characterCore, ownerTransform);
                break;

            case IdleSubState.Wandering:
                HandleIdleWandering(characterCore, ownerTransform);
                break;
        }
    }

    private void SetIdleSubState(IdleSubState newState, CharacterCore characterCore, Transform ownerTransform)
    {
        idleSubState = newState;

        switch (idleSubState)
        {
            case IdleSubState.Standing:
                idleStandingTimer = UnityEngine.Random.Range(idleStandMin, idleStandMax);
                break;

            case IdleSubState.Wandering:
                if (TryPickWanderPointInBase(ownerTransform.position, out wanderDestination))
                {
                    characterCore.MoveTo(wanderDestination);
                }
                else
                {
                    idleStandingTimer = 0.5f;
                    idleSubState = IdleSubState.Standing;
                }
                break;
        }
    }

    private void HandleIdleStanding(CharacterCore characterCore, Transform ownerTransform)
    {
        idleStandingTimer -= Time.deltaTime;
        if (idleStandingTimer <= 0f)
        {
            SetIdleSubState(IdleSubState.Wandering, characterCore, ownerTransform);
        }
    }

    private void HandleIdleWandering(CharacterCore characterCore, Transform ownerTransform)
    {
        if (Vector3.Distance(ownerTransform.position, wanderDestination) <= arriveDistance)
        {
            SetIdleSubState(IdleSubState.Standing, characterCore, ownerTransform);
        }
    }

    private bool TryPickWanderPointInBase(Vector3 currentPosition, out Vector3 result)
    {
        result = currentPosition;

        if (baseManager == null)
            return false;

        Vector3 center = baseManager.GetCenter();
        float radius = baseManager.GetRadius();

        for (int i = 0; i < 12; i++)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 candidate = new Vector3(center.x + randomOffset.x, center.y, center.z + randomOffset.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navmeshSampleMaxDistance, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        return false;
    }
}
