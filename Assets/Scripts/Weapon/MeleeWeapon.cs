using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Melee weapon type")]
    [SerializeField] private MeleeWeaponTypeSO meleeWeaponTypeSO;

    [Header("Hit Settings")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float radius = 0.25f;

    [SerializeField] private bool debugSpawnHitSphere = true;
    [SerializeField] private float debugSphereLifetime = 2f;

    private AudioSource audioSource;

    public Transform HitPoint => hitPoint;
    public float Radius => radius;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public List<Health> GetHitTargets(Transform attackerRoot)
    {
        var result = new List<Health>();

        if (hitPoint == null) return result;
        if (meleeWeaponTypeSO == null) return result;

        LayerMask mask = meleeWeaponTypeSO.targetableMask;

        Vector3 center = hitPoint.position;
        SpawnDebugSphere(center);

        Collider[] hits = Physics.OverlapSphere(hitPoint.position, radius, mask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return result;

        var unique = new HashSet<Health>();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col == null) continue;

            // nie bij siebie
            if (attackerRoot != null && col.transform.root == attackerRoot) continue;

            // Health zwykle jest na rodzicu, collider na dzieciach
            Health health = col.GetComponentInParent<Health>();
            if (health == null) continue;

            if (!unique.Add(health)) continue;

            result.Add(health);
        }

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint.position, radius);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(hitPoint.position, hitPoint.position + hitPoint.forward * 0.3f);
    }

    private void SpawnDebugSphere(Vector3 position)
    {
        if (!debugSpawnHitSphere) return;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * (radius * 2f);

        Destroy(sphere.GetComponent<Collider>());

        var renderer = sphere.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.red;

        Destroy(sphere, debugSphereLifetime);
    }

    public void PlayHitFleshSound()
    {
        if (audioSource == null) return;
        if (meleeWeaponTypeSO == null || meleeWeaponTypeSO.hitFleshClip == null) return;

        audioSource.PlayOneShot(meleeWeaponTypeSO.hitFleshClip);
    }
}