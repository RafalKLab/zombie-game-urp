using System;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    [SerializeField] private Faction faction;

    [SerializeField] private Transform center;
    [SerializeField] private float baseRadius;

    private BaseRadar baseRadar;
    private BaseSupplyManager baseSupplyManager;

    private void Awake()
    {
        baseRadar = GetComponent<BaseRadar>();
        baseSupplyManager = GetComponent<BaseSupplyManager>();
    }

    public Faction GetFaction() => faction;
    public BaseRadar GetBaseRadar() => baseRadar;
    public BaseSupplyManager GetBaseSupplyManager() => baseSupplyManager;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center.position, baseRadius);

    }
    public Vector3 GetCenter() => center != null ? center.position : transform.position;
    public float GetRadius() => baseRadius;
}
