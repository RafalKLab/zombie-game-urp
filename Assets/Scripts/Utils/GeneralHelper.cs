using UnityEngine;

public static class GeneralHelper {
    public static bool IsInRange(
    Vector3 origin,
    Vector3 target,
    float range,
    bool ignoreHeight = true,
    float buffer = 0.05f)
    {
        if (range <= 0f) return false;

        if (ignoreHeight)
        {
            origin.y = 0f;
            target.y = 0f;
        }

        float effectiveRange = range - buffer;
        if (effectiveRange < 0f) effectiveRange = range;

        float sqrDist = (target - origin).sqrMagnitude;
        return sqrDist <= effectiveRange * effectiveRange;
    }
}
