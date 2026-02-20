using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseManager))]
public class BaseRadar : MonoBehaviour
{
    [Header("Scan")]
    [SerializeField] private LayerMask targetableLayerMask;
    [SerializeField] private List<BaseRadarSensor> radarSensors = new();
    [SerializeField] private float scanInterval = 4f;

    [SerializeField] private bool debugLog = true;

    private BaseManager baseManager;
    private float scanTimer;

    // Dedup + cache
    private readonly Dictionary<int, RadarContact> contactsById = new();
    private readonly List<RadarContact> contactsSorted = new();
    private int requestIndex = 0;

    private void Awake()
    {
        baseManager = GetComponent<BaseManager>();
        scanTimer = scanInterval;
    }

    private void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer > 0f) return;
        scanTimer = scanInterval;

        PerformScan();
    }

    public bool TryRequestContact(out RadarContact contact)
    {
        contact = null;

        int contactCount = contactsSorted.Count;
        if (contactCount == 0)
        {
            requestIndex = 0;
            return false;
        }

        if (requestIndex < 0) requestIndex = 0;
        if (requestIndex >= contactCount) requestIndex = 0;

        for (int attempts = 0; attempts < contactCount; attempts++)
        {
            contactCount = contactsSorted.Count;
            if (contactCount == 0)
            {
                requestIndex = 0;
                return false;
            }

            if (requestIndex >= contactCount) requestIndex = 0;

            RadarContact candidateContact = contactsSorted[requestIndex];
            requestIndex++;

            if (IsValidContact(candidateContact))
            {
                contact = candidateContact;
                return true;
            }
        }

        return false;
    }

    private static bool IsValidContact(RadarContact contact)
    {
        if (contact == null) return false;
        if (contact.Target == null) return false;

        Health targetHealth = contact.Health != null
            ? contact.Health
            : contact.Target.GetComponentInParent<Health>();

        if (targetHealth == null) return false;
        if (targetHealth.IsDead) return false;

        return true;
    }

    public IReadOnlyList<RadarContact> GetContactsSorted() => contactsSorted;

    private void PerformScan()
    {
        contactsById.Clear();
        contactsSorted.Clear();

        Vector3 basePos = transform.position;

        for (int s = 0; s < radarSensors.Count; s++)
        {
            var sensor = radarSensors[s];
            if (sensor == null) continue;

            Collider[] hits = sensor.Scan(targetableLayerMask);
            if (hits == null || hits.Length == 0) continue;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i];
                if (col == null) continue;

                AiTarget aiTarget = col.GetComponentInParent<AiTarget>();
                if (aiTarget == null) continue;

                // 1) Pomijamy swoich (frakcja)
                if (aiTarget.GetFaction() == baseManager.GetFaction())
                    continue;

                // 2) Health check
                Health health = aiTarget.GetComponentInParent<Health>();
                if (health == null) continue;
                if (health.IsDead) continue;

                // 3) Unikalny klucz (dedup)
                int id = aiTarget.gameObject.GetInstanceID();

                // 4) Dystans
                Vector3 pos = aiTarget.transform.position;
                float distSqr = (pos - basePos).sqrMagnitude;

                if (contactsById.TryGetValue(id, out var existing))
                {
                    if (distSqr < existing.DistanceSqr)
                    {
                        existing.LastKnownPosition = pos;
                        existing.DistanceSqr = distSqr;
                        existing.LastSeenTime = Time.time;
                        existing.Target = aiTarget;
                        existing.Health = health;
                    }
                }
                else
                {
                    contactsById.Add(id, new RadarContact
                    {
                        Id = id,
                        Target = aiTarget,
                        Health = health,
                        LastKnownPosition = pos,
                        DistanceSqr = distSqr,
                        LastSeenTime = Time.time
                    });
                }
            }
        }

        // 5) Zrob liste i posortuj
        foreach (var kvp in contactsById)
            contactsSorted.Add(kvp.Value);

        contactsSorted.Sort((a, b) => a.DistanceSqr.CompareTo(b.DistanceSqr));

        if (debugLog)
        {
            int count = contactsSorted.Count;

            string sample = "";
            int preview = Mathf.Min(3, count);
            for (int i = 0; i < preview; i++)
            {
                float d = Mathf.Sqrt(contactsSorted[i].DistanceSqr);
                sample += (i == 0 ? "" : ", ") + d.ToString("F1") + "m";
            }

            Debug.Log($"[BaseRadar] Wykryto: {count} cel(i). Najblizsze dystanse: [{sample}]", this);
        }

        if (contactsSorted.Count == 0)
        {
            requestIndex = 0;
        }
        else
        {
            requestIndex %= contactsSorted.Count;
        }
    }

    [Serializable]
    public class RadarContact
    {
        public int Id;
        public AiTarget Target;
        public Health Health;
        public Vector3 LastKnownPosition;
        public float DistanceSqr;
        public float LastSeenTime;
    }
}
