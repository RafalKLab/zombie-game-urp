using System.Collections.Generic;
using UnityEngine;

public class InteractableWeaponCrate : MonoBehaviour, IInteractable
{
    [SerializeField] private int priority = 0;
    [SerializeField] private Transform uiAnchor;

    private readonly List<IInteractableAction> actions = new();

    public int Priority => priority;

    private void Awake()
    {
        actions.Clear();

        // Zbierz wszystkie komponenty na tym GO, ktore implementuja IInteractableAction
        var components = GetComponents<MonoBehaviour>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] is IInteractableAction a)
                actions.Add(a);
        }
    }

    public Transform GetUIAnchor() => uiAnchor != null ? uiAnchor : transform;

    public IReadOnlyList<IInteractableAction> GetActions(Interactor interactor) => actions;

    public List<IInteractableAction> GetExecutableActions(Interactor interactor)
    {
        var result = new List<IInteractableAction>(actions.Count);

        for (int i = 0; i < actions.Count; i++)
        {
            var a = actions[i];
            if (a == null) continue;
            if (!a.CanExecute(interactor)) continue;
            result.Add(a);
        }

        result.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return result;
    }

    public bool CanInteract(Interactor interactor)
    {
        return GetExecutableActions(interactor).Count > 0;
    }

    public string GetInteractPrompt(Interactor interactor)
    {
        var exec = GetExecutableActions(interactor);

        if (exec.Count == 0) return string.Empty;

        // Shortcut: 1 akcja => pokaz prompt tej akcji
        if (exec.Count == 1)
            return exec[0].GetExecutePrompt(interactor);

        return "F - Interact (choose)";
    }

    public InteractResult Interact(Interactor interactor)
    {
        var exec = GetExecutableActions(interactor);
        if (exec.Count == 0) return InteractResult.None;

        if (exec.Count == 1)
            return exec[0].Execute(interactor) ? InteractResult.Executed : InteractResult.None;

        return InteractResult.NeedsChoice;
    }
}