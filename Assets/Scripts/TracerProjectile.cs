using System.Collections;
using UnityEngine;

public sealed class TracerProjectile : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TrailRenderer trail;
    
    [Header("Flight (VFX)")]
    [Tooltip("Ensures the tracer is visible for at least a few frames (trail + bloom need time).")]
    [SerializeField] private float minTravelTime = 0.05f;

    [Tooltip("Safety timeout in case something goes wrong.")]
    [SerializeField] private float maxLifeTime = 1.5f;

    [Header("Despawn")]
    [Tooltip("How long to keep the object after reaching the end so the trail can fade out.")]
    [SerializeField] private float despawnAfterArrive = 0.25f;

    private Coroutine _moveRoutine;
    private TracerTypeSO tracerTypeSO;

    public void Init(Vector3 start, Vector3 end, TracerTypeSO tracerTypeSO)
    {
        this.tracerTypeSO = tracerTypeSO;

        transform.position = start;

        if (trail != null)
        {
            // Important when reusing / spawning quickly:
            trail.Clear();
            trail.emitting = true;
        }

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveRoutine(end));
    }

    private IEnumerator MoveRoutine(Vector3 end)
    {
        float startTime = Time.time;

        Vector3 start = transform.position;
        float dist = Vector3.Distance(start, end);

        if (dist < 0.01f)
        {
            ArriveAndDespawn();
            yield break;
        }

        // Key change: enforce minimum travel time so the tracer exists for several frames
        float travelTime = dist / Mathf.Max(0.01f, tracerTypeSO.tracerSpeed);
        travelTime = Mathf.Max(travelTime, minTravelTime);

        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            if (Time.time - startTime > maxLifeTime)
                break;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / travelTime);
            transform.position = Vector3.Lerp(start, end, t);

            yield return null;
        }

        transform.position = end;
        ArriveAndDespawn();
    }

    private void ArriveAndDespawn()
    {
        if (trail != null)
            trail.emitting = false;

        Destroy(gameObject, despawnAfterArrive);
    }
}
