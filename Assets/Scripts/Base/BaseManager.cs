using UnityEngine;

public class BaseManager : MonoBehaviour
{
    [SerializeField] private Transform center;
    [SerializeField] private float radius;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center.position, radius);
    }

    public Vector3 GetCenter() => center != null ? center.position : transform.position;
    public float GetRadius() => radius;
}
