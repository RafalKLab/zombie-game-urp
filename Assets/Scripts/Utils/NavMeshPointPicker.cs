using UnityEngine;
using UnityEngine.AI;

public static class NavMeshPointPicker
{
    public static bool TryGetRandomPoint(
        Vector3 center,
        float radius,
        float navMeshSampleMaxDistance,
        int attempts,
        int areaMask,
        out Vector3 result)
    {
        result = center;

        for (int i = 0; i < attempts; i++)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 candidate = new Vector3(center.x + randomOffset.x, center.y, center.z + randomOffset.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleMaxDistance, areaMask))
            {
                result = hit.position;
                return true;
            }
        }

        if (NavMesh.SamplePosition(center, out NavMeshHit centerHit, navMeshSampleMaxDistance, areaMask))
        {
            result = centerHit.position;
            return true;
        }

        return false;
    }

    public static bool TryGetRandomPointWithMinRadius(
        Vector3 center,
        float radius,
        float minRadiusFraction,
        float navMeshSampleMaxDistance,
        int attempts,
        int areaMask,
        out Vector3 result)
    {
        result = center;

        float minRadius = Mathf.Clamp01(minRadiusFraction) * radius;

        for (int i = 0; i < attempts; i++)
        {
            Vector2 direction = UnityEngine.Random.insideUnitCircle;
            if (direction.sqrMagnitude < 0.0001f)
                continue;

            direction.Normalize();

            float distance = UnityEngine.Random.Range(minRadius, radius);
            Vector2 randomOffset = direction * distance;

            Vector3 candidate = new Vector3(center.x + randomOffset.x, center.y, center.z + randomOffset.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleMaxDistance, areaMask))
            {
                result = hit.position;
                return true;
            }
        }

        if (NavMesh.SamplePosition(center, out NavMeshHit centerHit, navMeshSampleMaxDistance, areaMask))
        {
            result = centerHit.position;
            return true;
        }

        return false;
    }
}
