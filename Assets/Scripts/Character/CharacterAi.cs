using System;
using UnityEngine;

[RequireComponent(typeof(CharacterCore))]
public class CharacterAi : MonoBehaviour, ICharacterController
{
    public LayerMask targetableMask;

    private CharacterCore characterCore;

    [SerializeField] private LayerMask lineOfSightMask;
    private void Awake()
    {
        characterCore = GetComponent<CharacterCore>();
    }


    private void Update()
    {
        if (characterCore.HasAttackTarget() == false)
        {
            Collider[] hits = GetHitsSortedByDistance(transform.position, 100f, targetableMask);

            foreach (Collider hit in hits)
            {
                AiTarget target = hit.GetComponentInParent<AiTarget>();
                if (target == null) continue;

                // avoid self kill
                if (hit.transform.root == transform.root) continue;

                if (target.GetFaction() == Faction.Hostile) continue;

                if (!HasLineOfSightToTarget(target))
                {
                    continue;
                }

                Health targetHealth = target.GetComponentInParent<Health>();
                if (targetHealth == null) continue;
                if (targetHealth.IsDead) continue;

                characterCore.SetAttackTarget(target);

                return;
            }
        }
    }

    public void ClearAttackTarget()
    {
        throw new System.NotImplementedException();
    }

    public void MoveTo(Vector3 target)
    {
        throw new System.NotImplementedException();
    }

    public void SetAttackTarget(AiTarget target)
    {
        throw new System.NotImplementedException();
    }

    private Collider[] GetHitsSortedByDistance(Vector3 origin, float radius, LayerMask mask)
    {
        var hits = Physics.OverlapSphere(origin, radius, mask);

        Array.Sort(hits, (a, b) =>
        {
            float da = (a.transform.position - origin).sqrMagnitude;
            float db = (b.transform.position - origin).sqrMagnitude;
            return da.CompareTo(db);
        });

        return hits;
    }


    private bool HasLineOfSightToTarget(AiTarget target)
    {
        if (target == null) return false;

        Transform aim = target.GetAimPoint();
        if (aim == null) return false;

        if (characterCore.GetCameraLookAtPoint() == null) return false;
        Vector3 origin = characterCore.GetCameraLookAtPoint().position;
        Vector3 dest = aim.position;

        Vector3 dir = dest - origin;
        float dist = dir.magnitude;
        if (dist < 0.01f) return true;

        if (Physics.Raycast(origin, dir / dist, out RaycastHit hit, dist, lineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<AiTarget>() == target;
        }

        return false;
    }

    private void OnEnable()
    {
        characterCore.OnKilled += CharacterCore_OnKilled;
    }

    private void OnDisable()
    {
        characterCore.OnKilled -= CharacterCore_OnKilled;
    }

    private void CharacterCore_OnKilled(object sender, System.EventArgs e)
    {
        Destroy(gameObject);
    }
}
