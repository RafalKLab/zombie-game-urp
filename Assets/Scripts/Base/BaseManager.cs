using System;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    public event EventHandler<OnBaseDefendRequestEventArgs> OnBaseDefendRequest;
    public class OnBaseDefendRequestEventArgs : EventArgs
    {
        public Transform defendPoint;
        public float defendRadius;
    }

    [SerializeField] private Faction faction;

    [SerializeField] private Transform center;
    [SerializeField] private float baseRadius;

    [SerializeField] private Transform defendPoint;
    [SerializeField] private float defendRadius;

    private BaseRadar baseRadar;

    private void Awake()
    {
        baseRadar = GetComponent<BaseRadar>();
    }

    public Faction GetFaction() => faction;
    public BaseRadar GetBaseRadar() => baseRadar;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center.position, baseRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(defendPoint.position, defendRadius);
    }

    public Vector3 GetCenter() => center != null ? center.position : transform.position;
    public float GetRadius() => baseRadius;

    public void RequsetDefend()
    {
        OnBaseDefendRequest?.Invoke(this, new OnBaseDefendRequestEventArgs {
            defendPoint = this.defendPoint,
            defendRadius = this.defendRadius,
        });
    }
}
