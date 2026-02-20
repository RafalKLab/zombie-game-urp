using UnityEngine;

public class BaseRadarSensor : MonoBehaviour
{
    [SerializeField] private Transform antenna;
    [SerializeField] private float radius = 15f;

    public Collider[] Scan(LayerMask targetableMask)
    {
        Vector3 origin = antenna != null ? antenna.position : transform.position;
        return Physics.OverlapSphere(origin, radius, targetableMask);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = antenna != null ? antenna.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, radius);
    }
}
