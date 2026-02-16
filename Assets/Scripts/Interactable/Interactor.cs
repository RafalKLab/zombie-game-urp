using System;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    public event EventHandler<OnInteractableChangeEventArgs> OnInteractableChange;
    public class OnInteractableChangeEventArgs : EventArgs
    {
        public IInteractable interactable;
    }

    [SerializeField] float interactionRange = 1f;
    [SerializeField] LayerMask interactableLayerMask;
    [SerializeField] float interactableDetectCooldownMax = 0.5f;
    [SerializeField] private Transform interactionPivot;

    private IInteractable interactableCurrent;
    private float interactableDetectCooldown = 0f;

    public CharacterCore Character { get; private set; }

    private void Awake()
    {
        if (interactionPivot == null)
            interactionPivot = transform;

        Character = GetComponent<CharacterCore>();
    }
    private void Update()
    {
        interactableDetectCooldown -= Time.deltaTime;
        if (interactableDetectCooldown > 0) return;

        DetectInteractables();

        interactableDetectCooldown = interactableDetectCooldownMax;
    }

    private void DetectInteractables()
    {
        Vector3 origin = interactionPivot.position;

        Collider[] hits = Physics.OverlapSphere(
            origin,
            interactionRange,
            interactableLayerMask,
            QueryTriggerInteraction.Collide
        );

        IInteractable best = null;
        float bestDistance = float.MaxValue;
        int bestPriority = int.MinValue;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            if (!interactable.CanInteract(this)) continue;

            float distance = Vector3.Distance(origin, hit.transform.position);

            if (best == null)
            {
                best = interactable;
                bestDistance = distance;
                bestPriority = interactable.Priority;
                continue;
            }

            if (distance < bestDistance - 0.01f)
            {
                best = interactable;
                bestDistance = distance;
                bestPriority = interactable.Priority;
            }
            else if (Mathf.Abs(distance - bestDistance) < 0.2f)
            {
                if (interactable.Priority > bestPriority)
                {
                    best = interactable;
                    bestDistance = distance;
                    bestPriority = interactable.Priority;
                }
            }
        }

        SetCurrentInteractable(best);
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Transform pivot = interactionPivot != null ? interactionPivot : transform;

    //    if (interactionRange <= 0f) return;

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(pivot.position, interactionRange);
    //}

    public bool TryInteractCurrent()
    {
        if (interactableCurrent == null) return false;
        if (!interactableCurrent.CanInteract(this)) return false;

        bool success = interactableCurrent.Interact(this);

        if (success) {
            SetCurrentInteractable(null);
        }

        return success;
    }

    private void SetCurrentInteractable(IInteractable newInteractable)
    {
        if (ReferenceEquals(interactableCurrent, newInteractable)) return;

        interactableCurrent = newInteractable;
        OnInteractableChange?.Invoke(this,
            new OnInteractableChangeEventArgs { interactable = newInteractable });
    }

}
