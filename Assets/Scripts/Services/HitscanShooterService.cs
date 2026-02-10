using UnityEngine;

public sealed class HitscanShooterService
{
    private readonly RaycastHit[] _buffer;
    private readonly int _envLayer;

    public HitscanShooterService(int bufferSize, int envLayer)
    {
        _buffer = new RaycastHit[bufferSize];
        _envLayer = envLayer;
    }

    public ShotResult Shoot(
        Vector3 origin,
        Vector3 direction,
        WeaponTypeSO weaponType,
        Transform ignoreRoot,
        AiTarget activeTarget,
        bool debugDraw = false,
        float debugDuration = 0.1f)
    {
        var result = new ShotResult
        {
            Origin = origin,
            EndPoint = origin + direction * weaponType.range,
            DidHitSomething = false,
            ActiveTargetKilled = false
        };

        int hitCount = Physics.RaycastNonAlloc(
            origin, direction, _buffer,
            weaponType.range,
            weaponType.layerMask,
            QueryTriggerInteraction.Ignore
        );

        if (debugDraw)
            Debug.DrawRay(origin, direction * weaponType.range, Color.yellow, debugDuration);

        if (hitCount == 0) return result;

        SortByDistance(hitCount);

        int penetrationsDone = 0;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _buffer[i];

            if (hit.transform.root == ignoreRoot) continue;

            if (hit.collider.gameObject.layer == _envLayer)
            {
                result.EndPoint = hit.point;
                result.DidHitSomething = true;
                break;
            }

            var health = hit.collider.GetComponentInParent<Health>();
            if (health == null || health.IsDead) continue;

            var target = hit.collider.GetComponentInParent<AiTarget>();
            if (target == null) continue;

            result.EndPoint = hit.point;
            result.DidHitSomething = true;

            bool isActive = (target == activeTarget);

            health.TakeDamage(weaponType.damage);
            penetrationsDone++;

            if (isActive && health.IsDead)
                result.ActiveTargetKilled = true;

            if (penetrationsDone >= weaponType.maxPenetrations)
                break;
        }

        return result;
    }

    private void SortByDistance(int hitCount)
    {
        for (int i = 0; i < hitCount - 1; i++)
        {
            int min = i;
            float minDist = _buffer[i].distance;

            for (int j = i + 1; j < hitCount; j++)
            {
                float d = _buffer[j].distance;
                if (d < minDist)
                {
                    min = j;
                    minDist = d;
                }
            }

            if (min != i)
                (_buffer[i], _buffer[min]) = (_buffer[min], _buffer[i]);
        }
    }
}

public struct ShotResult
{
    public Vector3 Origin;
    public Vector3 EndPoint;
    public bool DidHitSomething;
    public bool ActiveTargetKilled;
}
