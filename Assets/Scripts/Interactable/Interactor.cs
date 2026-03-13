using System;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    public event EventHandler<OnInteractableChangeEventArgs> OnInteractableChange;
    public class OnInteractableChangeEventArgs : EventArgs
    {
        public IInteractable interactable;
    }

    public event EventHandler<OnInteractNeedsChoiceEventArgs> OnInteractNeedsChoice;
    public class OnInteractNeedsChoiceEventArgs : EventArgs
    {
        public IInteractable interactable;
    }

    private readonly List<IInteractableAction> choiceActions = new();
    public bool IsChoosing { get; private set; }

    [SerializeField] float interactionRange = 1f;
    [SerializeField] LayerMask interactableLayerMask;
    [SerializeField] float interactableDetectCooldownMax = 0.5f;
    [SerializeField] private Transform interactionPivot;

    private IInteractable interactableCurrent;
    private float interactableDetectCooldown = 0f;
    private IInteractableActionTwoStep pendingInteractableActionTwoStep;

    public Inventory Inventory{ get; private set; }
    public CharacterCore Character { get; private set; }

    private void Awake()
    {
        if (interactionPivot == null)
            interactionPivot = transform;

        Character = GetComponent<CharacterCore>();
    }

    private void Start()
    {
        Inventory = GetComponent<Inventory>();
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
            if (hit.transform.root == transform.root) continue;

            var interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;
            if (!interactable.CanInteract(this)) continue;

            float distance = Vector3.Distance(origin, hit.ClosestPoint(origin));
            int priority = interactable.Priority;

            //Debug.Log($"[Interactor] Candidate: {interactable} | Priority: {priority} | Distance: {distance:F2}");

            if (best == null)
            {
                best = interactable;
                bestDistance = distance;
                bestPriority = priority;

                //Debug.Log($"[Interactor] -> FIRST BEST: {best} | Priority: {bestPriority} | Distance: {bestDistance:F2}");
                continue;
            }

            if (priority > bestPriority)
            {
                best = interactable;
                bestDistance = distance;
                bestPriority = priority;

                //Debug.Log($"[Interactor] -> REPLACED BY PRIORITY: {best} | Priority: {bestPriority} | Distance: {bestDistance:F2}");
                continue;
            }

            if (priority == bestPriority && distance < bestDistance)
            {
                best = interactable;
                bestDistance = distance;
                bestPriority = priority;

                //Debug.Log($"[Interactor] -> REPLACED BY DISTANCE: {best} | Priority: {bestPriority} | Distance: {bestDistance:F2}");
            }
        }

        //Debug.Log($"[Interactor] FINAL BEST: {best} | Priority: {bestPriority} | Distance: {bestDistance:F2}");

        SetCurrentInteractable(best);
    }

    public bool TryInteractCurrent()
    {
        if (interactableCurrent == null) return false;
        if (!interactableCurrent.CanInteract(this)) return false;

        InteractResult result = interactableCurrent.Interact(this);

        if (result == InteractResult.Executed)
        {
            IsChoosing = false;
            SetCurrentInteractable(null);
            return true;
        }

        if (result == InteractResult.NeedsChoice)
        {
            IsChoosing = true;
            BuildChoiceList();

            OnInteractNeedsChoice?.Invoke(this,
                new OnInteractNeedsChoiceEventArgs { interactable = interactableCurrent });

            return false;
        }

        IsChoosing = false;
        return false;
    }

    public bool TryInteractCurrentAction(int index)
    {
        if (!IsChoosing) return false;
        if (interactableCurrent == null) return false;

        if (index < 0 || index >= choiceActions.Count) return false;

        var action = choiceActions[index];
        if (action == null) return false;

        if (!action.CanExecute(this)) return false;

        bool ok = action.Execute(this);

        if (ok)
        {
            IsChoosing = false;
            SetCurrentInteractable(null);
        }

        return ok;
    }

    private void SetCurrentInteractable(IInteractable newInteractable)
    {
        if (ReferenceEquals(interactableCurrent, newInteractable)) return;

        interactableCurrent = newInteractable;
        OnInteractableChange?.Invoke(this,
            new OnInteractableChangeEventArgs { interactable = newInteractable });
    }

    public IInteractable GetCurrentInteractable()
    {
        return interactableCurrent;
    }

    private void BuildChoiceList()
    {
        choiceActions.Clear();

        if (interactableCurrent == null) return;

        var all = interactableCurrent.GetActions(this);
        if (all == null) return;

        for (int i = 0; i < all.Count; i++)
        {
            var a = all[i];
            if (a != null)
                choiceActions.Add(a);
        }

        choiceActions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public IReadOnlyList<IInteractableAction> GetCurrentChoiceActions()
    {
        return choiceActions;
    }
    public void SetPendingInteractableActionTwoStep(IInteractableActionTwoStep action)
    {
        pendingInteractableActionTwoStep = action;
    }

    public bool TryFinalizePendingTwoStepAction()
    {
        if (pendingInteractableActionTwoStep == null) return false;

        bool success = pendingInteractableActionTwoStep.FinalizeExecute(this);
        pendingInteractableActionTwoStep = null;

        return success;
    }
}