using UnityEngine;

public static class AimSpreadService
{
    public static Vector3 ApplyConeSpread(Vector3 baseDir, float spreadAngleDeg)
    {
        if (spreadAngleDeg <= 0f) return baseDir;

        Vector2 offset = Random.insideUnitCircle * spreadAngleDeg;

        Vector3 right = Vector3.Cross(Vector3.up, baseDir);
        if (right.sqrMagnitude < 1e-6f)
            right = Vector3.Cross(Vector3.forward, baseDir);
        right.Normalize();

        Vector3 up = Vector3.Cross(baseDir, right);

        Quaternion yaw = Quaternion.AngleAxis(offset.x, up);
        Quaternion pitch = Quaternion.AngleAxis(-offset.y, right);

        return (yaw * pitch) * baseDir;
    }
}
